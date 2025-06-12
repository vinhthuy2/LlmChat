using LlmChat.Chat;

namespace LlmChat.Infra.Data;

public interface IChatSessionRepository
{
    Task<ChatSession?> GetSessionAsync(Guid id);
    Task SaveSessionAsync(Guid id, string content);
    Task UpdateSessionAsync(Guid id, string content);
    Task DeleteSessionAsync(Guid id);
} 