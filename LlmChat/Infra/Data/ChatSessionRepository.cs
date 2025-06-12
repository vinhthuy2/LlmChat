using LlmChat.Chat;
using LlmChat.Infra.Logging;
using Microsoft.EntityFrameworkCore;

namespace LlmChat.Infra.Data;

public class ChatSessionRepository : IChatSessionRepository
{
    private readonly AppDbContext _dbContext;
    private readonly ILoggingService _logger;

    public ChatSessionRepository(AppDbContext dbContext, ILoggingService logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<ChatSession?> GetSessionAsync(Guid id)
    {
        _logger.LogDebug("Getting session {SessionId}", id);
        return await _dbContext.ChatSessions.FindAsync(id);
    }

    public async Task SaveSessionAsync(Guid id, string content)
    {
        _logger.LogDebug("Saving new session {SessionId}", id);
        var session = new ChatSession { Id = id, Content = content };
        await _dbContext.ChatSessions.AddAsync(session);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateSessionAsync(Guid id, string content)
    {
        _logger.LogDebug("Updating session {SessionId}", id);
        var session = await _dbContext.ChatSessions.FindAsync(id);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found for update", id);
            throw new InvalidOperationException($"Session {id} not found");
        }

        session.Content = content;
        await _dbContext.SaveChangesAsync();
    }

    public async Task DeleteSessionAsync(Guid id)
    {
        _logger.LogDebug("Deleting session {SessionId}", id);
        var session = await _dbContext.ChatSessions.FindAsync(id);
        if (session == null)
        {
            _logger.LogWarning("Session {SessionId} not found for deletion", id);
            return;
        }

        _dbContext.ChatSessions.Remove(session);
        await _dbContext.SaveChangesAsync();
    }
} 