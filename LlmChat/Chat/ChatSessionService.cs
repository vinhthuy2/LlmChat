using LlmChat.Infra.Logging;

namespace LlmChat.Chat;

public class ChatSessionService(IChatSessionStore store, ILoggingService logger) : IChatSessionService
{
    public async Task<ChatSession?> GetSessionAsync(Guid id)
    {
        logger.LogInformation("Getting session {SessionId}", id);
        return await store.GetSessionAsync(id);
    }

    public async Task SaveSessionAsync(Guid id, string content)
    {
        logger.LogInformation("Saving session {SessionId}", id);
        var existingSession = await store.GetSessionAsync(id);
        
        if (existingSession == null)
        {
            await store.SaveSessionAsync(id, content);
        }
        else
        {
            await store.UpdateSessionAsync(id, content);
        }
    }

    public async Task<IReadOnlyList<ChatSession>> GetSessions()
    {
        logger.LogInformation("Getting all sessions");
        return await store.GetSessionsAsync();
    }
}