namespace LlmChat.Agents;

public interface ILlmSupervisory
{
    Task<string> ReviseAsync(string sentence, Guid sessionId, string? extraSystemPrompt = null, bool includeHistory = false);
}