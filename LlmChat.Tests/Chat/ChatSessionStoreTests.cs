using LlmChat.Chat;
using LlmChat.Infra.Data;
using LlmChat.Infra.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace LlmChat.Tests.Chat;

[TestClass]
public class ChatSessionStoreTests
{
    private IServiceProvider _serviceProvider = null!;
    private ILoggingService _loggingService = null!;
    private ChatSessionStore _store = null!;

    [TestInitialize]
    public void Setup()
    {
        var services = new ServiceCollection();
        var dbName = "ChatSessionStoreTestsDb";
        services.AddDbContext<AppDbContext>(opts => opts.UseInMemoryDatabase(dbName));
        _loggingService = Substitute.For<ILoggingService>();
        services.AddSingleton(_loggingService);
        _serviceProvider = services.BuildServiceProvider();
        _store = new ChatSessionStore(_serviceProvider, _loggingService);
    }

    [TestCleanup]
    public void Cleanup()
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Dispose();
    }

    [TestMethod]
    public async Task GetSessionAsync_WithExistingSession_ReturnsSession()
    {
        // Arrange
        var sessionId = Guid.NewGuid();
        var session = new ChatSession { Id = sessionId, Content = "test content" };
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.ChatSessions.AddAsync(session);
            await dbContext.SaveChangesAsync();
        }

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
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var savedSession = await dbContext.ChatSessions.FindAsync(sessionId);
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
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.ChatSessions.AddAsync(session);
            await dbContext.SaveChangesAsync();
        }

        // Act
        await _store.UpdateSessionAsync(sessionId, "new content");

        // Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var updatedSession = await dbContext.ChatSessions.FindAsync(sessionId);
            Assert.IsNotNull(updatedSession);
            Assert.AreEqual("new content", updatedSession.Content);
        }
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
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.ChatSessions.AddAsync(session);
            await dbContext.SaveChangesAsync();
        }

        // Act
        await _store.DeleteSessionAsync(sessionId);

        // Assert
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var deletedSession = await dbContext.ChatSessions.FindAsync(sessionId);
            Assert.IsNull(deletedSession);
        }
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
        using (var scope = _serviceProvider.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await dbContext.ChatSessions.AddRangeAsync(sessions);
            await dbContext.SaveChangesAsync();
        }

        // Act
        var result = await _store.GetSessionsAsync();

        // Assert
        Assert.AreEqual(3, result.Count);
        CollectionAssert.AreEquivalent(sessions.Select(s => s.Id).ToList(), result.Select(s => s.Id).ToList());
        _loggingService.Received().LogDebug(Arg.Is<string>(s => s.Contains("Getting all sessions")));
    }
} 