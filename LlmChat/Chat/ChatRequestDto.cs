namespace LlmChat.Chat;

public record ChatRequestDto(
    string Content,
    string Sender,
    Guid? SessionId,
    DateTimeOffset Timestamp);