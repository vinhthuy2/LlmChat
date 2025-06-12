using LlmChat.Infra.Data;
using LlmChat.Infra.Logging;
using Microsoft.EntityFrameworkCore;

namespace LlmChat.Chat;

public class ChatSessionStore : IChatSessionStore
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggingService _loggingService;

    public ChatSessionStore(IServiceProvider serviceProvider, ILoggingService loggingService)
    {
        _serviceProvider = serviceProvider;
        _loggingService = loggingService;
    }

    public async Task<ChatSession?> GetSessionAsync(Guid id)
    {
        _loggingService.LogDebug("Getting session {SessionId}", id);
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await dbContext.ChatSessions.FindAsync(id);
    }

    public async Task SaveSessionAsync(Guid id, string content)
    {
        _loggingService.LogDebug("Saving new session {SessionId}", id);
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var session = new ChatSession { Id = id, Content = content };
        await dbContext.ChatSessions.AddAsync(session);
        await dbContext.SaveChangesAsync();
    }

    public async Task UpdateSessionAsync(Guid id, string content)
    {
        _loggingService.LogDebug("Updating session {SessionId}", id);
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var session = await dbContext.ChatSessions.FindAsync(id);
        if (session == null)
        {
            _loggingService.LogWarning("Session {SessionId} not found for update", id);
            throw new InvalidOperationException($"Session {id} not found");
        }
        session.Content = content;
        await dbContext.SaveChangesAsync();
    }

    public async Task DeleteSessionAsync(Guid id)
    {
        _loggingService.LogDebug("Deleting session {SessionId}", id);
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var session = await dbContext.ChatSessions.FindAsync(id);
        if (session != null)
        {
            dbContext.ChatSessions.Remove(session);
            await dbContext.SaveChangesAsync();
        }
        else
        {
            _loggingService.LogWarning("Session {SessionId} not found for deletion", id);
        }
    }

    public async Task<IReadOnlyList<ChatSession>> GetSessionsAsync()
    {
        _loggingService.LogDebug("Getting all sessions");
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await dbContext.ChatSessions.ToListAsync();
    }
} 