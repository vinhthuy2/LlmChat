using LlmChat.Agents;
using LlmChat.Chat;
using LlmChat.Infra.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace LlmChat.Tests.Agents;

[TestClass]
public class OllamaAgentTests
{
    private IChatSessionStore _store = null!;
    private ILoggingService _loggingService = null!;
    private OllamaAgent _agent = null!;

    [TestInitialize]
    public void Setup()
    {
        _store = Substitute.For<IChatSessionStore>();
        _loggingService = Substitute.For<ILoggingService>();
        _agent = new OllamaAgent(_store, _loggingService);
    }

    [TestMethod]
    public async Task Answer_WithNewSession_CreatesNewSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var question = "test question";
        _store.GetSessionAsync(sessionId).Returns((ChatSession?)null);

        // Act
        var result = await _agent.Answer(question, sessionId);

        // Assert
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
        _store.GetSessionAsync(sessionId).Returns(existingSession);

        // Act
        var result = await _agent.Answer(question, sessionId);

        // Assert
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Processing answer")), sessionId);
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Answer completed")), sessionId);
    }

    [TestMethod]
    public void DeferAMessage_StoresMessageForLater()
    {
        // Arrange
        var message = "deferred message";

        // Act
        _agent.DeferAMessage(message, Guid.NewGuid());

        // Assert
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Deferring message")));
    }

    [TestMethod]
    public async Task StreamedAnswer_WithPendingMessage_ReturnsStream()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var message = "deferred message";
        _agent.DeferAMessage(message, sessionId);
        _store.GetSessionAsync(sessionId).Returns((ChatSession?)null);

        // Act
        var stream = await _agent.StreamedAnswer(sessionId);

        // Assert
        Assert.IsNotNull(stream);
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Processing deferred message")), sessionId);
    }
}