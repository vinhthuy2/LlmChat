namespace LlmChat.Chat;

public class ChatSession
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
    public DateTimeOffset LastUpdated { get; private set; } = DateTimeOffset.UtcNow;

    public IReadOnlyList<Message> Messages => _messages;
    private readonly List<Message> _messages = new();

    public void AddMessage(Message message)
    {
        ArgumentNullException.ThrowIfNull(message);
        if (message.SessionId != Id)
        {
            throw new InvalidOperationException("Message session ID does not match chat session ID.");
        }

        _messages.Add(message);
        LastUpdated = DateTimeOffset.UtcNow;
    }
}