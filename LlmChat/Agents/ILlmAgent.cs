namespace LlmChat.Agents;

public interface ILlmAgent
{
    Task<string> AnswerAsync(string sentence, Guid sessionId);

    void DeferAMessageAsync(string sentence, Guid sessionId);

    Task<IAsyncEnumerable<string>> StreamedAnswerAsync(Guid sessionId);
}