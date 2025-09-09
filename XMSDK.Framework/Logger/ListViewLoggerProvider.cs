using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using XMSDK.Framework.Forms;

namespace XMSDK.Framework.Logger;

public class ListViewLoggerProvider(LoggerList loggerList) : ILoggerProvider
{
    private readonly ConcurrentDictionary<string, ListViewLogger> _loggers = new();

    public ILogger CreateLogger(string categoryName)
    {
        return _loggers.GetOrAdd(categoryName, name => new ListViewLogger(name, loggerList));
    }

    public void Dispose()
    {
        _loggers.Clear();
    }
}

public class ListViewLogger(string name, LoggerList loggerList) : ILogger
{

    public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        var message = formatter(state, exception);
        if (exception != null)
        {
            message += Environment.NewLine + exception;
        }

        loggerList.AddLogEntry(DateTime.Now, logLevel, message, name);
    }
}