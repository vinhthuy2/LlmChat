namespace LlmChat.Chat;

public record ChatResponseDto(
    string Content,
    string Sender,
    Guid SessionId);