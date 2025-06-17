using OllamaSharp;

namespace LlmChat.Agents;

public static class OllamaClientFactory
{
    private const string LlmHost = "http://localhost:11434";

    public static IOllamaApiClient Phi3Client()
    {
        return new OllamaApiClient(new Uri(LlmHost), "phi3:mini");
    }
}