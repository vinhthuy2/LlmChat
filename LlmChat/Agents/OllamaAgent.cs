using LlmChat.Chat;
using LlmChat.Infra.Logging;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace LlmChat.Agents;

public class OllamaAgent : ILlmAgent
{
    private const string Uri = "http://localhost:11434";
    private const string Model = "phi3";
    private const string SystemPrompt = "You are a helpful assistant. Answer the user's questions based on the provided context.";
    private readonly OllamaApiClient _client;
    private readonly Dictionary<Guid, OllamaSharp.Chat> _conversationHistory = new();
    private readonly Dictionary<Guid, string> _pendingMessages = new();
    private readonly IChatSessionStore _store;
    private readonly ILoggingService _logger;

    public OllamaAgent(IChatSessionStore store, ILoggingService logger)
    {
        _store = store;
        _logger = logger;
        _client = new OllamaApiClient(new Uri(Uri));
        _client.SelectedModel = Model;
        _logger.LogInformation("OllamaAgent initialized with model {Model}", Model);
    }

    public async Task<string> Answer(string question, Guid sessionId)
    {
        _logger.LogInformation("Processing answer for session {SessionId}", sessionId);
        var conversation = await GetOllamaChat(sessionId);
        var response = await conversation.SendAsAsync("user", question).StreamToEndAsync();

        await SaveSession(sessionId);
        _logger.LogInformation("Answer completed for session {SessionId}", sessionId);
        return response;
    }

    public void DeferAMessage(string message)
    {
        _logger.LogInformation("Deferring message for later processing");
        _pendingMessages[Guid.NewGuid()] = message;
    }

    public async Task<IAsyncEnumerable<string>> StreamedAnswer(Guid sessionId)
    {
        if (!_pendingMessages.TryGetValue(sessionId, out var message))
        {
            throw new InvalidOperationException($"No pending message found for session {sessionId}");
        }

        _logger.LogInformation("Processing deferred message for session {SessionId}", sessionId);
        var conversation = await GetOllamaChat(sessionId);
        _pendingMessages.Remove(sessionId);
        return conversation.SendAsAsync("user", message);
    }

    private async Task<OllamaSharp.Chat> GetOllamaChat(Guid sessionId)
    {
        return _conversationHistory.TryGetValue(sessionId, out var conversation)
            ? await Task.FromResult(conversation)
            : await LoadSession(sessionId);
    }

    private OllamaSharp.Chat CreateOllamaChat(ChatSession session)
    {
        _logger.LogDebug("Creating Ollama chat from session {SessionId}", session.Id);
        var messages = session.Content.Split("|")
            .Select(m => m.Split(":", 2))
            .Select(ss => new Message(ss[0], ss[1]))
            .ToList();

        return new OllamaSharp.Chat(_client, SystemPrompt)
        {
            Messages = messages
        };
    }

    private async Task<OllamaSharp.Chat> LoadSession(Guid sessionId)
    {
        _logger.LogInformation("Loading session {SessionId}", sessionId);
        var session = await _store.GetSessionAsync(sessionId);

        var conversation = session != null
            ? CreateOllamaChat(session)
            : new OllamaSharp.Chat(_client, SystemPrompt);

        _conversationHistory[sessionId] = conversation;
        _logger.LogInformation("Session {SessionId} loaded successfully", sessionId);
        return conversation;
    }

    private async Task SaveSession(Guid sessionId)
    {
        if (!_conversationHistory.ContainsKey(sessionId))
        {
            var ex = new InvalidOperationException("Session not loaded or does not exist.");
            _logger.LogError(ex, "Failed to save session {SessionId}: Session not loaded or does not exist", sessionId);
            throw ex;
        }

        _logger.LogInformation("Saving session {SessionId}", sessionId);
        var content = string.Join("|", _conversationHistory[sessionId].Messages.Select(m => $"{m.Role}:{m.Content}"));
        await _store.SaveSessionAsync(sessionId, content);
    }
}