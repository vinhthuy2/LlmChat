using LlmChat.Chat;
using LlmChat.Infra.Logging;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace LlmChat.Agents;

public class OllamaSupervisory(IOllamaApiClient chatClient, IChatSessionService chatSessionService, ILoggingService logger) : ILlmSupervisory
{
    private const string SystemPrompt =
        "Revise my sentence based on the conversation history, if provided. " +
        "Strictly preserve the specified sentiment, if provided. " +
        "Minimize changes to maintain closeness to my original sentence. " +
        "Respond only with the revised sentence in markdown, without additional information or explanation. " +
        "Response format: `Revised: <revied_content>`";

    public Task<string> ReviseAsync(string sentence, Guid sessionId, string? extraSystemPrompt,  bool includeHistory = false)
    {
        return SendMessage(sentence, sessionId, extraSystemPrompt, includeHistory).StreamToEndAsync();
    }

    private async IAsyncEnumerable<string> SendMessage(string sentence, Guid sessionId, string? extraSystemPrompt = null, bool includeHistory = false)
    {
        var request = new ChatRequest
        {
            Stream = true,
            Options = new RequestOptions()
            {
                Temperature = (float)0.1,
            },
        };

        List<Message> messages = [new Message(ChatRole.System, SystemPrompt)];

        var history = await GetSessionHistory(sessionId);

        if (history is not null)
        {
            messages.Add(new Message(ChatRole.System, history));
        }

        if (extraSystemPrompt != null)
        {
            messages.Add(new Message(ChatRole.System, extraSystemPrompt));
        }

        messages.Add(new(ChatRole.User, sentence));

        request.Messages = messages;

        await foreach (var answer in chatClient.ChatAsync(request).ConfigureAwait(false))
        {
            if (answer is null)
                continue;

            yield return answer.Message.Content ?? string.Empty;
        }
    }

    private async Task<string?> GetSessionHistory(Guid sessionId)
    {
        var chatSession = await chatSessionService.GetSessionAsync(sessionId);

        return chatSession is null
            ? null
            : $"Conversation History: {chatSession.Content}";
    }
}