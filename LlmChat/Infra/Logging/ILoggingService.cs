using Microsoft.Extensions.Logging;

namespace LlmChat.Infra.Logging;

public interface ILoggingService
{
    void LogInformation(string message, params object[] args);
    void LogWarning(string message, params object[] args);
    void LogError(Exception ex, string message, params object[] args);
    void LogDebug(string message, params object[] args);
    IDisposable BeginScope<TState>(TState state) where TState : notnull;
} 