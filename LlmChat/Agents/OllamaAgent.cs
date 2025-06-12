using LlmChat.Chat;
using OllamaSharp;

namespace LlmChat.Agents;

public class OllamaAgent : ILlmAgent
{
    private const string Uri = "http://localhost:11434";
    private const string Model = "phi3";
    private const string SystemPrompt = "You are a helpful assistant. Answer the user's questions based on the provided context.";
    private readonly OllamaApiClient _client;
    private Dictionary<Guid, OllamaSharp.Chat> _conversationHistory = new();

    public OllamaAgent(IChatSessionService chatSessionService)
    {
        _client = new OllamaApiClient(new Uri(Uri));
        _client.SelectedModel = Model;

        var sessions = chatSessionService.GetSessions();
        foreach (var session in sessions.Result)
        {
            if (_conversationHistory.ContainsKey(session.Id)) continue;

            var conversation = new OllamaSharp.Chat(_client, SystemPrompt)
            {
                Messages = session.Messages
                    .Select(m =>
                        new OllamaSharp.Models.Chat.Message
                        {
                            Role = m.Sender == "user" ? "user" : "assistant", Content = m.Content,
                        }
                    )
                    .ToList()
            };
            _conversationHistory[session.Id] = conversation;
        }
    }

    public async Task<string> Answer(Guid sessionId, string question)
    {
        if (!_conversationHistory.TryGetValue(sessionId, out var conversation))
        {
            conversation = new OllamaSharp.Chat(_client, SystemPrompt);
            _conversationHistory[sessionId] = conversation;
        }

        return await conversation.SendAsAsync("user", question).StreamToEndAsync();
    }
}