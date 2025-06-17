using FluentAssertions;
using LlmChat.Agents;
using LlmChat.Chat;
using LlmChat.Infra.Logging;
using NSubstitute;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace LlmChat.Tests.Agents;

[TestClass]
public class OllamaAgentTests
{
    private readonly IChatSessionService _chatSessionService = Substitute.For<IChatSessionService>();
    private readonly ILoggingService _loggingService =  Substitute.For<ILoggingService>();
    private readonly IOllamaApiClient _ollamaApiClient = Substitute.For<IOllamaApiClient>();
    private readonly OllamaAgent sut;

    public OllamaAgentTests()
    {
        _ollamaApiClient.ChatAsync(Arg.Any<ChatRequest>()).ReturnsForAnyArgs(GetLlmResponses());
        sut = new OllamaAgent(_chatSessionService, _ollamaApiClient, _loggingService);
    }

    [TestMethod]
    public async Task Answer_WithNewSession_CreatesNewSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var question = "test question";
        _chatSessionService.GetSessionAsync(sessionId).Returns((ChatSession?)null);

        // Act
        var result = await sut.AnswerAsync(question, sessionId);

        // Assert
        result.Should().Be("hello from llm agent.");
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Processing answer")), sessionId);
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Answer completed")), sessionId);
    }

    [TestMethod]
    public async Task Answer_WithExistingSession_UsesExistingSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var question = "test question";
        var existingSession = new ChatSession { Id = sessionId, Content = "assistant:Hello|user:Hi" };
        _chatSessionService.GetSessionAsync(sessionId).Returns(existingSession);

        // Act
        var result = await sut.AnswerAsync(question, sessionId);

        // Assert
        result.Should().Be("hello from llm agent.");
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Processing answer")), sessionId);
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Answer completed")), sessionId);
    }

    [TestMethod]
    public void DeferAMessage_StoresMessageForLater()
    {
        // Arrange
        var message = "deferred message";

        // Act
        sut.DeferAMessageAsync(message, Guid.NewGuid());

        // Assert
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Deferring message")));
    }

    [TestMethod]
    public async Task StreamedAnswer_WithPendingMessage_ReturnsStream()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var message = "deferred message";
        sut.DeferAMessageAsync(message, sessionId);
        _chatSessionService.GetSessionAsync(sessionId).Returns((ChatSession?)null);

        // Act
        var stream = await sut.StreamedAnswerAsync(sessionId);

        // Assert
        stream.Should().NotBeNull();

        var result = await stream.StreamToEndAsync();
        result.Should().Be("hello from llm agent.");

        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Processing deferred message")), sessionId);
    }

    public static async IAsyncEnumerable<ChatResponseStream?> GetLlmResponses()
    {
        yield return new ChatResponseStream()
        {
            Message = new Message(ChatRole.Assistant, "hello")
        };

        yield return new ChatResponseStream()
        {
            Message = new Message(ChatRole.Assistant, " from")
        };

        yield return new ChatResponseStream()
        {
            Message = new Message(ChatRole.Assistant, " llm agent."),
            Done = true
        };

        await Task.CompletedTask; // to make the compiler warning go away
    }
}