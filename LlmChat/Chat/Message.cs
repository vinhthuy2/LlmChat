namespace LlmChat.Chat;

public record Message(Guid Id, string Content, string Sender, Guid SessionId, DateTimeOffset Timestamp);