using LlmChat.Infra.Logging;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace LlmChat.Tests.Infra;

[TestClass]
public class LoggingServiceTests
{
    private ILogger<LoggingService> _logger = null!;
    private LoggingService _service = null!;

    [TestInitialize]
    public void Setup()
    {
        _logger = Substitute.For<ILogger<LoggingService>>();
        _service = new LoggingService(_logger);
    }

    [TestMethod]
    public void LogInformation_LogsWithCorrectLevel()
    {
        // Arrange
        var message = "Test info message";
        var args = new object[] { "arg1" };

        // Act
        _service.LogInformation(message, args);

        // Assert
        _logger.Received().Log(LogLevel.Information, Arg.Any<EventId>(), Arg.Is<object[]>(a => a[0].ToString() == message), Arg.Any<Exception>(), Arg.Any<Func<object, Exception?, string>>());
    }

    [TestMethod]
    public void LogWarning_LogsWithCorrectLevel()
    {
        // Arrange
        var message = "Test warning message";
        var args = new object[] { "arg1" };

        // Act
        _service.LogWarning(message, args);

        // Assert
        _logger.Received().Log(LogLevel.Warning, Arg.Any<EventId>(), Arg.Is<object[]>(a => a[0].ToString() == message), Arg.Any<Exception>(), Arg.Any<Func<object, Exception?, string>>());
    }

    [TestMethod]
    public void LogError_LogsWithCorrectLevel()
    {
        // Arrange
        var message = "Test error message";
        var exception = new Exception("Test exception");
        var args = new object[] { "arg1" };

        // Act
        _service.LogError(exception, message, args);

        // Assert
        _logger.Received().Log(LogLevel.Error, Arg.Any<EventId>(), Arg.Is<object[]>(a => a[0].ToString() == message), Arg.Is<Exception>(e => e == exception), Arg.Any<Func<object, Exception?, string>>());
    }

    [TestMethod]
    public void LogDebug_LogsWithCorrectLevel()
    {
        // Arrange
        var message = "Test debug message";
        var args = new object[] { "arg1" };

        // Act
        _service.LogDebug(message, args);

        // Assert
        _logger.Received().Log(LogLevel.Debug, Arg.Any<EventId>(), Arg.Is<object[]>(a => a[0].ToString() == message), Arg.Any<Exception>(), Arg.Any<Func<object, Exception?, string>>());
    }

    [TestMethod]
    public void BeginScope_ReturnsLoggerScope()
    {
        // Arrange
        var state = new { Test = "value" };
        var scope = Substitute.For<IDisposable>();
        _logger.BeginScope(state).Returns(scope);

        // Act
        var result = _service.BeginScope(state);

        // Assert
        Assert.AreEqual(scope, result);
        _logger.Received().BeginScope(state);
    }
} 