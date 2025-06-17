using LlmChat.Infra.Logging;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace LlmChat.Agents;

public class OllamaSupervisory(IOllamaApiClient chatClient, ILoggingService logger) : ILlmAgent
{
    private const string SystemPrompt =
        "Revise my sentence. " +
        "Strictly preserve the specified sentiment, if provided. " +
        "Minimize changes to maintain closeness to my original sentence. " +
        "Respond only with the revised sentence in markdown, without additional information or explanation. " +
        "Response format: `Revised: <revied_content>`";

    private readonly Dictionary<Guid, string> _pendingMessages = new();
    private readonly Dictionary<Guid, string?> _pendingExtraSystemPrompt = new();

    public Task<string> Answer(string question, Guid sessionId, string? extraSystemPrompt)
    {
        return SendMessage(question, extraSystemPrompt).StreamToEndAsync();
    }

    public void DeferAMessage(string question, Guid sessionId, string? extraSystemPrompt)   {
        logger.LogInformation("Deferring message for later processing");
        _pendingMessages[sessionId] = question;
        _pendingExtraSystemPrompt[sessionId] = extraSystemPrompt;
    }

    public Task<IAsyncEnumerable<string>> StreamedAnswer(Guid sessionId)
    {
        if (!_pendingMessages.Remove(sessionId, out var message))
        {
            var ex = new InvalidOperationException($"No pending message found for session {sessionId}");
            logger.LogError(ex, "Failed to stream answer: No pending message for session {SessionId}", sessionId);
            throw ex;
        }

        if (!_pendingExtraSystemPrompt.Remove(sessionId, out var extraSystemPrompt))
        {
            var ex = new InvalidOperationException($"No pending system prompt found for session {sessionId}");
            logger.LogError(ex, "Failed to stream answer: No pending system prompt for session {SessionId}", sessionId);
            throw ex;
        }

        logger.LogInformation("Processing deferred message for session {SessionId}", sessionId);
       return Task.FromResult(SendMessage(message, extraSystemPrompt));
    }

    private async IAsyncEnumerable<string> SendMessage(string question, string? extraSystemPrompt = null)
    {
        var request = new ChatRequest
        {
            Messages = [new Message(ChatRole.System, SystemPrompt)],
            Stream = true,
            Options = new RequestOptions()
            {
                Temperature = (float)0.1,
            },
        };

        if (extraSystemPrompt != null)
        {
            request.Messages.Append(new(ChatRole.System, extraSystemPrompt));
        }

        request.Messages.Append(new(ChatRole.User, question));

        await foreach (var answer in chatClient.ChatAsync(request).ConfigureAwait(false))
        {
            if (answer is null)
                continue;

            yield return answer.Message.Content ?? string.Empty;
        }
    }
}