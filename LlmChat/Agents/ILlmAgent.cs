namespace LlmChat.Agents;

public interface ILlmAgent
{
    Task<string> Answer(string question, Guid sessionId);

    void DeferAMessage(string question, Guid sessionId);

    Task<IAsyncEnumerable<string>> StreamedAnswer(Guid sessionId);
}