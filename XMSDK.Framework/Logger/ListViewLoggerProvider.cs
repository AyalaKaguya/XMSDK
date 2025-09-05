using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using XMSDK.Framework.Forms;

namespace XMSDK.Framework.Logger
{
    public class ListViewLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ListViewLogger> _loggers =
            new ConcurrentDictionary<string, ListViewLogger>();

        private readonly LoggerList _loggerList;

        public ListViewLoggerProvider(LoggerList loggerList)
        {
            _loggerList = loggerList ?? throw new ArgumentNullException(nameof(loggerList));
        }

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new ListViewLogger(name, _loggerList));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }

    public class ListViewLogger : ILogger
    {
        private readonly string _name;
        private readonly LoggerList _loggerList;

        public ListViewLogger(string name, LoggerList loggerList)
        {
            _name = name;
            _loggerList = loggerList;
        }

        public IDisposable BeginScope<TState>(TState state) => null!;

        public bool IsEnabled(LogLevel logLevel) => logLevel != LogLevel.None;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            if (exception != null)
            {
                message += Environment.NewLine + exception;
            }

            _loggerList.AddLogEntry(DateTime.Now, logLevel, message, _name);
        }
    }
}