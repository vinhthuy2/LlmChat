namespace LlmChat.Agents;

public interface ILlmAgent
{
    Task<string> Answer(string question, Guid sessionId, string? extraSystemPrompt = null);

    void DeferAMessage(string question, Guid sessionId, string? extraSystemPrompt = null);

    Task<IAsyncEnumerable<string>> StreamedAnswer(Guid sessionId);
}