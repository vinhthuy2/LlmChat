using LlmChat.Chat;
using LlmChat.Infra.Data;
using LlmChat.Infra.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace LlmChat.Tests.Chat;

[TestClass]
public class ChatSessionStoreTests
{
    private AppDbContext _dbContext = null!;
    private ILoggingService _loggingService = null!;
    private ChatSessionStore _store = null!;

    [TestInitialize]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _loggingService = Substitute.For<ILoggingService>();
        _store = new ChatSessionStore(_dbContext, _loggingService);
    }

    [TestCleanup]
    public void Cleanup()
    {
        _dbContext.Database.EnsureDeleted();
        _dbContext.Dispose();
    }

    [TestMethod]
    public async Task GetSessionAsync_WithExistingSession_ReturnsSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ChatSession { Id = sessionId, Content = "test content" };
        await _dbContext.ChatSessions.AddAsync(session);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _store.GetSessionAsync(sessionId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(sessionId, result.Id);
        Assert.AreEqual("test content", result.Content);
        _loggingService.Received().LogDebug(Arg.Is<string>(s => s.Contains("Getting session")), sessionId);
    }

    [TestMethod]
    public async Task GetSessionAsync_WithNonExistingSession_ReturnsNull()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        var result = await _store.GetSessionAsync(sessionId);

        // Assert
        Assert.IsNull(result);
        _loggingService.Received().LogDebug(Arg.Is<string>(s => s.Contains("Getting session")), sessionId);
    }

    [TestMethod]
    public async Task SaveSessionAsync_CreatesNewSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var content = "new session content";

        // Act
        await _store.SaveSessionAsync(sessionId, content);

        // Assert
        var savedSession = await _dbContext.ChatSessions.FindAsync(sessionId);
        Assert.IsNotNull(savedSession);
        Assert.AreEqual(content, savedSession.Content);
        _loggingService.Received().LogDebug(Arg.Is<string>(s => s.Contains("Saving new session")), sessionId);
    }

    [TestMethod]
    public async Task UpdateSessionAsync_WithExistingSession_UpdatesSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ChatSession { Id = sessionId, Content = "old content" };
        await _dbContext.ChatSessions.AddAsync(session);
        await _dbContext.SaveChangesAsync();

        // Act
        await _store.UpdateSessionAsync(sessionId, "new content");

        // Assert
        var updatedSession = await _dbContext.ChatSessions.FindAsync(sessionId);
        Assert.IsNotNull(updatedSession);
        Assert.AreEqual("new content", updatedSession.Content);
        _loggingService.Received().LogDebug(Arg.Is<string>(s => s.Contains("Updating session")), sessionId);
    }

    [TestMethod]
    public async Task UpdateSessionAsync_WithNonExistingSession_ThrowsException()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => 
            _store.UpdateSessionAsync(sessionId, "new content"));
        _loggingService.Received().LogWarning(Arg.Is<string>(s => s.Contains("not found for update")), sessionId);
    }

    [TestMethod]
    public async Task DeleteSessionAsync_WithExistingSession_DeletesSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ChatSession { Id = sessionId, Content = "test content" };
        await _dbContext.ChatSessions.AddAsync(session);
        await _dbContext.SaveChangesAsync();

        // Act
        await _store.DeleteSessionAsync(sessionId);

        // Assert
        var deletedSession = await _dbContext.ChatSessions.FindAsync(sessionId);
        Assert.IsNull(deletedSession);
        _loggingService.Received().LogDebug(Arg.Is<string>(s => s.Contains("Deleting session")), sessionId);
    }

    [TestMethod]
    public async Task DeleteSessionAsync_WithNonExistingSession_DoesNothing()
    {
        // Arrange
        var sessionId = Guid.NewGuid();

        // Act
        await _store.DeleteSessionAsync(sessionId);

        // Assert
        _loggingService.Received().LogWarning(Arg.Is<string>(s => s.Contains("not found for deletion")), sessionId);
    }

    [TestMethod]
    public async Task GetSessionsAsync_ReturnsAllSessions()
    {
        // Arrange
        var sessions = new[]
        {
            new ChatSession { Id = Guid.NewGuid(), Content = "content 1" },
            new ChatSession { Id = Guid.NewGuid(), Content = "content 2" },
            new ChatSession { Id = Guid.NewGuid(), Content = "content 3" }
        };
        await _dbContext.ChatSessions.AddRangeAsync(sessions);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _store.GetSessionsAsync();

        // Assert
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEquivalent(sessions.Select(s => s.Id).ToList(), result.Select(s => s.Id).ToList());
        _loggingService.Received().LogDebug(Arg.Is<string>(s => s.Contains("Getting all sessions")));
    }
} 