using LlmChat.Chat;
using LlmChat.Infra.Logging;
using NSubstitute;

namespace LlmChat.Tests.Chat;

[TestClass]
public class ChatSessionServiceTests
{
    private IChatSessionStore _store = null!;
    private ILoggingService _loggingService = null!;
    private ChatSessionService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _store = Substitute.For<IChatSessionStore>();
        _loggingService = Substitute.For<ILoggingService>();
        _service = new ChatSessionService(_store, _loggingService);
    }

    [TestMethod]
    public async Task GetSession_WithExistingSession_ReturnsSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ChatSession { Id = sessionId, Content = "test content" };
        _store.GetSessionAsync(sessionId).Returns(session);

        // Act
        var result = await _service.GetSession(sessionId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(sessionId, result.Id);
        Assert.AreEqual("test content", result.Content);
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Getting session")), sessionId);
    }

    [TestMethod]
    public async Task GetSession_WithNonExistingSession_ReturnsNull()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        _store.GetSessionAsync(sessionId).Returns((ChatSession?)null);

        // Act
        var result = await _service.GetSession(sessionId);

        // Assert
        Assert.IsNull(result);
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Getting session")), sessionId);
    }

    [TestMethod]
    public async Task SaveSession_WithNewSession_CreatesSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var content = "new session content";
        _store.GetSessionAsync(sessionId).Returns((ChatSession?)null);

        // Act
        await _service.SaveSession(sessionId, content);

        // Assert
        await _store.Received().SaveSessionAsync(sessionId, content);
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Saving session")), sessionId);
    }

    [TestMethod]
    public async Task SaveSession_WithExistingSession_UpdatesSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var existingSession = new ChatSession { Id = sessionId, Content = "old content" };
        _store.GetSessionAsync(sessionId).Returns(existingSession);

        // Act
        await _service.SaveSession(sessionId, "new content");

        // Assert
        await _store.Received().UpdateSessionAsync(sessionId, "new content");
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Saving session")), sessionId);
    }

    [TestMethod]
    public async Task GetSessions_ReturnsAllSessions()
    {
        // Arrange
        var sessions = new[]
        {
            new ChatSession { Id = Guid.NewGuid(), Content = "content 1" },
            new ChatSession { Id = Guid.NewGuid(), Content = "content 2" },
            new ChatSession { Id = Guid.NewGuid(), Content = "content 3" }
        };
        _store.GetSessionsAsync().Returns(sessions);

        // Act
        var result = await _service.GetSessions();

        // Assert
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEquivalent(sessions.Select(s => s.Id).ToList(), result.Select(s => s.Id).ToList());
        _loggingService.Received().LogInformation(Arg.Is<string>(s => s.Contains("Getting all sessions")));
    }
} 