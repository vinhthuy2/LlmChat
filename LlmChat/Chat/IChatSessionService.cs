namespace LlmChat.Chat;

public interface IChatSessionService
{
    Task<ChatSession?> GetSession(Guid sessionId);

    Task<IReadOnlyList<ChatSession>> GetSessions();
}