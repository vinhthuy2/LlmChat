namespace LlmChat.Chat;

public interface IChatSessionService
{
    Task<ChatSession?> GetSession(Guid sessionId);
    Task SaveSession(Guid sessionId, string sessionContent);
}