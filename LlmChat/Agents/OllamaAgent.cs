using LlmChat.Chat;
using LlmChat.Infra.Logging;
using OllamaSharp;
using OllamaSharp.Models.Chat;

namespace LlmChat.Agents;

public class OllamaAgent(IChatSessionService chatSessionService, IOllamaApiClient chatClient, ILoggingService logger) : ILlmAgent
{
    private const string SystemPrompt =
        "You are an english learning buddy. " +
        "We're playing role play. ";
    private readonly Dictionary<Guid, OllamaSharp.Chat> _conversationHistory = new();
    private readonly Dictionary<Guid, string> _pendingMessages = new();

    public async Task<string> AnswerAsync(string sentence, Guid sessionId)
    {
        logger.LogInformation("Processing answer for session {SessionId}", sessionId);
        var conversation = await GetOllamaChat(sessionId);
        var response = await conversation.SendAsAsync("user", sentence).StreamToEndAsync();

        await SaveSession(sessionId);
        logger.LogInformation("Answer completed for session {SessionId}", sessionId);
        return response;
    }

    public void DeferAMessageAsync(string sentence, Guid sessionId)
    {
        logger.LogInformation("Deferring message for later processing");
        _pendingMessages[sessionId] = sentence;
    }

    public async Task<IAsyncEnumerable<string>> StreamedAnswerAsync(Guid sessionId)
    {
        if (!_pendingMessages.Remove(sessionId, out var message))
        {
            var ex = new InvalidOperationException($"No pending message found for session {sessionId}");
            logger.LogError(ex, "Failed to stream answer: No pending message for session {SessionId}", sessionId);
            throw ex;
        }

        logger.LogInformation("Processing deferred message for session {SessionId}", sessionId);
        var conversation = await GetOllamaChat(sessionId);
        return conversation.SendAsAsync("user", message);
    }

    private async Task<OllamaSharp.Chat> GetOllamaChat(Guid sessionId)
    {
        return _conversationHistory.TryGetValue(sessionId, out var conversation)
            ? await Task.FromResult(conversation)
            : await LoadSession(sessionId);
    }

    private async Task<OllamaSharp.Chat> LoadSession(Guid sessionId)
    {
        logger.LogInformation("Loading session {SessionId}", sessionId);
        var conversation = new OllamaSharp.Chat(chatClient, SystemPrompt);

        var session = await chatSessionService.GetSessionAsync(sessionId);
        if (session != null)
        {
            conversation.Messages = session.Content.Split("|")
                .Select(m => m.Split(":", 2))
                .Select(ss => new Message(ss[0], ss[1]))
                .ToList();
        }

        _conversationHistory[sessionId] = conversation;
        logger.LogInformation("Session {SessionId} loaded successfully", sessionId);
        return conversation;
    }

    private async Task SaveSession(Guid sessionId)
    {
        if (!_conversationHistory.ContainsKey(sessionId))
        {
            var ex = new InvalidOperationException("Session not loaded or does not exist.");
            logger.LogError(ex, "Failed to save session {SessionId}: Session not loaded or does not exist", sessionId);
            throw ex;
        }

        logger.LogInformation("Saving session {SessionId}", sessionId);
        var content = string.Join("|", _conversationHistory[sessionId].Messages.Select(m => $"{m.Role}:{m.Content}"));
        await chatSessionService.SaveSessionAsync(sessionId, content);
    }
}