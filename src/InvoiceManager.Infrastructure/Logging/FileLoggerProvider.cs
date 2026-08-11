using Microsoft.Extensions.Logging;

namespace InvoiceManager.Infrastructure.Logging;

public sealed class FileLoggerProvider : ILoggerProvider
{
    private readonly Lock _syncRoot = new();
    private readonly StreamWriter _writer;
    private readonly TimeProvider _timeProvider;
    private bool _isDisposed;

    public FileLoggerProvider(string logsDirectory, TimeProvider timeProvider)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logsDirectory);
        ArgumentNullException.ThrowIfNull(timeProvider);

        Directory.CreateDirectory(logsDirectory);

        var logPath = Path.Combine(logsDirectory, "invoice-manager.log");
        var stream = new FileStream(logPath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite);

        _writer = new StreamWriter(stream) { AutoFlush = true };
        _timeProvider = timeProvider;
    }

    public ILogger CreateLogger(string categoryName)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        return new FileLogger(this, categoryName);
    }

    public void Dispose()
    {
        lock (_syncRoot)
        {
            if (_isDisposed)
            {
                return;
            }

            _writer.Dispose();
            _isDisposed = true;
        }
    }

    private void Write(string categoryName, LogLevel logLevel, EventId eventId, string message, Exception? exception)
    {
        var timestamp = _timeProvider.GetUtcNow();
        var eventText = eventId.Id == 0 ? string.Empty : $" [{eventId.Id}]";
        var exceptionText = exception is null ? string.Empty : $"{Environment.NewLine}{exception}";
        var line = $"{timestamp:O} [{logLevel}] {categoryName}{eventText}: {message}{exceptionText}";

        lock (_syncRoot)
        {
            if (!_isDisposed)
            {
                _writer.WriteLine(line);
            }
        }
    }

    private sealed class FileLogger(FileLoggerProvider provider, string categoryName) : ILogger
    {
        public IDisposable? BeginScope<TState>(TState state)
            where TState : notnull
        {
            return NullScope.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
        {
            ArgumentNullException.ThrowIfNull(formatter);

            if (IsEnabled(logLevel))
            {
                provider.Write(categoryName, logLevel, eventId, formatter(state, exception), exception);
            }
        }
    }

    private sealed class NullScope : IDisposable
    {
        public static NullScope Instance { get; } = new();

        public void Dispose()
        {
        }
    }
}
