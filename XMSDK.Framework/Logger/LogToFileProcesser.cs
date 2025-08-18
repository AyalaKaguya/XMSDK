using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XMSDK.Framework.Logger
{
    public class LogToFileProcesser : ITraceMessageProcesser, IDisposable
    {
        private readonly string _logFilePath;
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private readonly CancellationTokenSource _logCancellationTokenSource = new CancellationTokenSource();
        private readonly Task _logWriterTask;

        public LogToFileProcesser(string logFilePath)
        {
            _logFilePath = logFilePath;
            _logWriterTask = Task.Run(WriteLogToFileTask, _logCancellationTokenSource.Token);
        }

        public void OnMessage(string msg)
        {
            _logQueue.Enqueue(msg);
        }

        private async Task WriteLogToFileTask()
        {
            while (!_logCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var logsToWrite = new List<string>();

                    // 批量获取日志消息
                    while (_logQueue.TryDequeue(out string logMessage) && logsToWrite.Count < 100)
                    {
                        logsToWrite.Add(logMessage);
                    }

                    if (logsToWrite.Count > 0)
                    {
                        using (var sw = new StreamWriter(_logFilePath, true, Encoding.UTF8))
                        {
                            foreach (var log in logsToWrite)
                            {
                                await sw.WriteLineAsync(log);
                            }

                            await sw.FlushAsync();
                        }
                    }

                    await Task.Delay(100, _logCancellationTokenSource.Token); // 如果没有日志，稍作等待
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    // 避免日志记录异常导致程序崩溃
                    Console.WriteLine($"日志记录异常: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            _logCancellationTokenSource.Cancel();
            _logWriterTask.Wait();
            _logCancellationTokenSource?.Dispose();
            _logWriterTask?.Dispose();
        }
    }
}