namespace LlmChat.Agents;

public interface ILlmAgent
{
    Task<string> Answer(Guid sessionId, string question);
}