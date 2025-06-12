namespace LlmChat.Chat;

public record ChatResponseDto(
    Guid MessageId,
    string Content,
    string Sender,
    Guid SessionId,
    DateTimeOffset Timestamp);