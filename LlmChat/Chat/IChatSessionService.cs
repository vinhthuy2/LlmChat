namespace LlmChat.Chat;

public interface IChatSessionService
{
    Task<ChatSession?> GetSessionAsync(Guid sessionId);
    Task SaveSessionAsync(Guid sessionId, string sessionContent);
}