namespace LlmChat.Chat;

public record OriginalMessageDto(
    Guid? SessionId,
    string Content,
    string? ExtraSystemPrompt);