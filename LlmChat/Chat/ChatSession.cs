using System.ComponentModel.DataAnnotations;

namespace LlmChat.Chat;

public class ChatSession
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    [MaxLength(10000)]
    public string Content { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; private set; } = DateTimeOffset.UtcNow;

    public void AddMessage(string role, string message)
    {
        Content += $"|{role}:{message}";
    }
}