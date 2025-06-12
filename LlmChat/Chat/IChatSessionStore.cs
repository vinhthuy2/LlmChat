using LlmChat.Chat;

namespace LlmChat.Chat;

public interface IChatSessionStore
{
    Task<ChatSession?> GetSessionAsync(Guid id);
    Task SaveSessionAsync(Guid id, string content);
    Task UpdateSessionAsync(Guid id, string content);
    Task DeleteSessionAsync(Guid id);
    Task<IReadOnlyList<ChatSession>> GetSessionsAsync();
} 