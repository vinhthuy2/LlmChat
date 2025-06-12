using LlmChat.Infra.Logging;

namespace LlmChat.Chat;

public class ChatSessionService : IChatSessionService
{
    private readonly IChatSessionStore _store;
    private readonly ILoggingService _logger;

    public ChatSessionService(IChatSessionStore store, ILoggingService logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task<ChatSession?> GetSession(Guid id)
    {
        _logger.LogInformation("Getting session {SessionId}", id);
        return await _store.GetSessionAsync(id);
    }

    public async Task SaveSession(Guid id, string content)
    {
        _logger.LogInformation("Saving session {SessionId}", id);
        var existingSession = await _store.GetSessionAsync(id);
        
        if (existingSession == null)
        {
            await _store.SaveSessionAsync(id, content);
        }
        else
        {
            await _store.UpdateSessionAsync(id, content);
        }
    }

    public async Task<IReadOnlyList<ChatSession>> GetSessions()
    {
        _logger.LogInformation("Getting all sessions");
        return await _store.GetSessionsAsync();
    }
}