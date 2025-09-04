using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace XMSDK.Framework.Logger
{
    /// <summary>
    /// 将日志记录到指定文件夹中，默认按天生成日志文件
    /// </summary>
    public class LogToFolderProcesser : ITraceMessageProcesser, IDisposable
    {
        private readonly string _logFolderPath;
        private readonly ConcurrentQueue<string> _logQueue = new ConcurrentQueue<string>();
        private readonly CancellationTokenSource _logCancellationTokenSource = new CancellationTokenSource();
        private readonly Task _logWriterTask;

        public LogToFolderProcesser(string logFolderPath)
        {
            _logFolderPath = logFolderPath;
            _logWriterTask = Task.Run(WriteLogToFileTask, _logCancellationTokenSource.Token);
        }

        public void OnMessage(string msg)
        {
            _logQueue.Enqueue(msg);
        }

        private Func<string, string> _logFileNameGenerator  = folderPath => Path.Combine(folderPath, $"{DateTime.Now:yyyyMMdd}.log");
        
        /// <summary>
        /// 自定义日志文件名生成器，参数为日志文件夹路径，返回值为完整的日志文件路径
        /// 默认按天生成日志文件，格式为yyyyMMdd.log
        /// </summary>
        /// <param name="generator"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public LogToFolderProcesser SetLogFileNameGenerator(Func<string, string> generator)
        {
            _logFileNameGenerator = generator ?? throw new ArgumentNullException(nameof(generator));
            return this;
        }

        private async Task WriteLogToFileTask()
        {
            if (!Directory.Exists(_logFolderPath))
            {
                Directory.CreateDirectory(_logFolderPath);
            }

            while (!_logCancellationTokenSource.Token.IsCancellationRequested)
            {
                try
                {
                    var logsToWrite = new List<string>();

                    // 批量获取日志消息
                    while (_logQueue.TryDequeue(out var logMessage) && logsToWrite.Count < 100)
                    {
                        logsToWrite.Add(logMessage);
                    }

                    if (logsToWrite.Count > 0)
                    {
                        using (var sw = new StreamWriter(_logFileNameGenerator(_logFolderPath), true, Encoding.UTF8))
                        {
                            foreach (var log in logsToWrite)
                            {
                                await sw.WriteLineAsync(log);
                            }

                            await sw.FlushAsync();
                        }
                    }

                    // 每100毫秒检查一次队列
                    await Task.Delay(100, _logCancellationTokenSource.Token);
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
                finally
                {
                    using (var sw = new StreamWriter(_logFileNameGenerator(_logFolderPath), true, Encoding.UTF8))
                    {
                        while (_logQueue.TryDequeue(out var logMessage))
                        {
                            await sw.WriteLineAsync(logMessage);
                        }
                        // 确保所有日志都被写入
                        await sw.FlushAsync();
                    }
                }
            }
        }

        public void Dispose()
        {
            _logCancellationTokenSource.Cancel();
            _logWriterTask.Wait();
            _logCancellationTokenSource.Dispose();
            _logWriterTask.Dispose();
        }
    }
}