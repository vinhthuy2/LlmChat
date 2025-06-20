using LlmChat.Chat;
using LlmChat.Infra.Logging;
using OllamaSharp;
using OllamaSharp.Models;
using OllamaSharp.Models.Chat;

namespace LlmChat.Agents;

public class OllamaSupervisory(IOllamaApiClient chatClient, IChatSessionService chatSessionService, ILoggingService logger) : ILlmSupervisory
{
    private const string SystemPrompt =
        "Please provide a revised and grammatically accurate rendition of my initial inquiry without additional commentary or follow-ups. "
        + "You solely provide the polished sentence as requested.\n";

    public Task<string> ReviseAsync(string sentence, Guid? sessionId, string? extraSystemPrompt, bool includeHistory = false)
    {
        return SendMessage(sentence, sessionId, extraSystemPrompt, includeHistory).StreamToEndAsync();
    }

    private async IAsyncEnumerable<string> SendMessage(string sentence, Guid? sessionId, string? extraSystemPrompt = null, bool includeHistory = false)
    {
        var request = new ChatRequest
        {
            Stream = true,
            Options = new RequestOptions()
            {
                Temperature = (float)0.1,
            },
        };

        List<Message> messages = [new(ChatRole.System, SystemPrompt)];

        var history = "";

        if (includeHistory && sessionId.HasValue)
        {
            history = await GetSessionHistory(sessionId.Value);
        }

        var m = $$"""
            ```json
            {
                "toBeRevised":
                {
                    "sentence": "{{sentence}}",
                    "sentiment": "{{extraSystemPrompt ?? "none"}}",
                    "conversation_history": "{{history ?? "none"}}"
                }
            }
            ```
            """;

        messages.Add(new Message(ChatRole.User, m));

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
            : $"Conversation History: {chatSession.Content}\n";
    }
}