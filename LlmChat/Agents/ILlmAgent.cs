namespace LlmChat.Agents;

public interface ILlmAgent
{
    Task<string> Answer(string question, Guid sessionId);

    void DeferAMessage(string message);

    Task<IAsyncEnumerable<string>> StreamedAnswer(Guid sessionId);
}