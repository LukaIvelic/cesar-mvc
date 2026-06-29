using System.Collections.Concurrent;

namespace cesar.Extensions;

public static class FileLoggerExtensions
{
    public static ILoggingBuilder AddCesarFileLogger(
        this ILoggingBuilder builder,
        IConfiguration configuration)
    {
        var path = configuration["Logging:File:Path"];
        if (string.IsNullOrWhiteSpace(path))
        {
            path = Path.Combine(AppContext.BaseDirectory, "logs", "cesar.log");
        }

        builder.AddProvider(new CesarFileLoggerProvider(path));
        return builder;
    }
}

public sealed class CesarFileLoggerProvider : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, CesarFileLogger> _loggers = new();
    private readonly string _path;
    private readonly object _sync = new();

    public CesarFileLoggerProvider(string path)
    {
        _path = Path.GetFullPath(path);
        Directory.CreateDirectory(Path.GetDirectoryName(_path) ?? AppContext.BaseDirectory);
    }

    public ILogger CreateLogger(string categoryName) =>
        _loggers.GetOrAdd(categoryName, name => new CesarFileLogger(name, _path, _sync));

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public sealed class CesarFileLogger : ILogger
{
    private readonly string _categoryName;
    private readonly string _path;
    private readonly object _sync;

    public CesarFileLogger(string categoryName, string path, object sync)
    {
        _categoryName = categoryName;
        _path = path;
        _sync = sync;
    }

    public IDisposable? BeginScope<TState>(TState state)
        where TState : notnull =>
        null;

    public bool IsEnabled(LogLevel logLevel) => logLevel >= LogLevel.Information;

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
        {
            return;
        }

        var message = formatter(state, exception);
        if (string.IsNullOrWhiteSpace(message) && exception is null)
        {
            return;
        }

        var line = $"{DateTimeOffset.UtcNow:O} [{logLevel}] {_categoryName} {message}";
        if (exception is not null)
        {
            line += Environment.NewLine + exception;
        }

        lock (_sync)
        {
            File.AppendAllText(_path, line + Environment.NewLine);
        }
    }
}
