using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace XMSDK.Framework.EventBus
{
    /// <summary>
    /// 异步事件总线，
    /// 适用于大量且需等待的事件处理，
    /// 通过异步队列处理事件，
    /// 可以提高性能和响应速度。
    /// 继承自 DirectBus 以复用订阅管理和路由匹配功能。
    /// </summary>
    public class AsyncBus : DirectBus, IDisposable
    {
        /// <summary>
        /// 异步事件处理委托
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="context">事件上下文</param>
        public delegate Task AsyncEventHandler<T>(EventContext<T> context);

        /// <summary>
        /// 异步异常处理回调委托
        /// </summary>
        /// <param name="context">事件上下文（object类型以支持泛型）</param>
        /// <param name="exception">发生的异常</param>
        /// <param name="handler">出错的处理器</param>
        /// <returns>是否继续抛出异常，true表示重新抛出，false表示忽略异常</returns>
        public delegate bool AsyncExceptionHandler(object context, Exception exception, Delegate handler);

        /// <summary>
        /// 事件队列项
        /// </summary>
        private class EventQueueItem
        {
            public object Context { get; set; }
            public Type PayloadType { get; set; }
            public TaskCompletionSource<int> CompletionSource { get; set; }
            public AsyncExceptionHandler AsyncExceptionHandler { get; set; }
        }

        private readonly ChannelWriter<EventQueueItem> _writer;
        private readonly ChannelReader<EventQueueItem> _reader;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _processingTask;
        private bool _disposed;

        public AsyncBus(int capacity = 1000)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                FullMode = BoundedChannelFullMode.Wait,
                SingleReader = true,
                SingleWriter = false
            };

            var eventQueue = Channel.CreateBounded<EventQueueItem>(options);
            _writer = eventQueue.Writer;
            _reader = eventQueue.Reader;
            _cancellationTokenSource = new CancellationTokenSource();

            // 启动后台处理任务
            _processingTask = Task.Run(ProcessEventsAsync);
        }

        #region 异步订阅方法

        /// <summary>
        /// 订阅异步事件（基于事件负载类型）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="handler">异步事件处理器</param>
        /// <returns>订阅ID，可用于取消订阅</returns>
        public string SubscribeAsync<T>(AsyncEventHandler<T> handler)
        {
            return Subscribe<T>(ctx => ConvertToAsyncHandler(handler, ctx));
        }

        /// <summary>
        /// 订阅异步事件（基于路由键精确匹配）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="routeKey">路由键</param>
        /// <param name="handler">异步事件处理器</param>
        /// <returns>订阅ID，可用于取消订阅</returns>
        public string SubscribeAsync<T>(string routeKey, AsyncEventHandler<T> handler)
        {
            return Subscribe<T>(routeKey, ctx => ConvertToAsyncHandler(handler, ctx));
        }

        /// <summary>
        /// 订阅异步事件（基于路由键模式匹配）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="routePattern">路由模式（支持通配符*和?，或正则表达式）</param>
        /// <param name="handler">异步事件处理器</param>
        /// <param name="isRegex">是否使用正则表达式匹配</param>
        /// <returns>订阅ID，可用于取消订阅</returns>
        public string SubscribeAsync<T>(string routePattern, AsyncEventHandler<T> handler, bool isRegex)
        {
            return Subscribe<T>(routePattern, ctx => ConvertToAsyncHandler(handler, ctx), isRegex);
        }

        /// <summary>
        /// 将异步处理器转换为同步处理器（用于存储）
        /// </summary>
        private static void ConvertToAsyncHandler<T>(AsyncEventHandler<T> asyncHandler, EventContext<T> context)
        {
            // 在同步上下文中启动异步任务，但不等待（Fire-and-forget）
            // 实际的异步执行将在队列处理中进行
            Task.Run(async () => await asyncHandler(context));
        }

        #endregion

        #region 异步发布方法
        
        /// <summary>
        /// 异步发布事件
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="payload">事件负载</param>
        /// <returns>处理该事件的处理器数量</returns>
        public async Task<int> PublishAsync<T>(T payload)
        {
            return await PublishAsync(payload, typeof(T).FullName);
        }

        /// <summary>
        /// 异步发布事件（带异常处理）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="payload">事件负载</param>
        /// <param name="onException">异常处理回调</param>
        /// <returns>处理该事件的处理器数量</returns>
        public async Task<int> PublishAsync<T>(T payload, AsyncExceptionHandler onException)
        {
            return await PublishAsync(payload, typeof(T).FullName, onException);
        }

        /// <summary>
        /// 异步发布事件
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="payload">事件负载</param>
        /// <param name="routeKey">路由键</param>
        /// <returns>处理该事件的处理器数量</returns>
        public async Task<int> PublishAsync<T>(T payload, string routeKey)
        {
            return await PublishAsync(payload, routeKey, null);
        }

        /// <summary>
        /// 异步发布事件（带异常处理）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="payload">事件负载</param>
        /// <param name="routeKey">路由键</param>
        /// <param name="onException">异常处理回调</param>
        /// <returns>处理该事件的处理器数量</returns>
        public async Task<int> PublishAsync<T>(T payload, string routeKey, AsyncExceptionHandler onException)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (_disposed) throw new ObjectDisposedException(nameof(AsyncBus));

            var context = new EventContext<T>(payload, routeKey);
            var completionSource = new TaskCompletionSource<int>();

            var queueItem = new EventQueueItem
            {
                Context = context,
                PayloadType = typeof(T),
                CompletionSource = completionSource,
                AsyncExceptionHandler = onException
            };

            await _writer.WriteAsync(queueItem, _cancellationTokenSource.Token);
            return await completionSource.Task;
        }

        /// <summary>
        /// 同步发布事件（非阻塞）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="payload">事件负载</param>
        /// <returns>是否成功加入队列</returns>
        public bool PublishFireAndForget<T>(T payload)
        {
            return PublishFireAndForget(payload, typeof(T).FullName);
        }

        /// <summary>
        /// 同步发布事件（非阻塞）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="payload">事件负载</param>
        /// <param name="routeKey">路由键</param>
        /// <returns>是否成功加入队列</returns>
        public bool PublishFireAndForget<T>(T payload, string routeKey)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (_disposed) return false;

            var context = new EventContext<T>(payload, routeKey);
            var completionSource = new TaskCompletionSource<int>();

            var queueItem = new EventQueueItem
            {
                Context = context,
                PayloadType = typeof(T),
                CompletionSource = completionSource,
                AsyncExceptionHandler = null
            };

            return _writer.TryWrite(queueItem);
        }

        #endregion

        #region 后台处理

        private async Task ProcessEventsAsync()
        {
            await foreach (var item in _reader.ReadAllAsync(_cancellationTokenSource.Token))
            {
                try
                {
                    var handledCount = await ProcessEventItem(item);
                    item.CompletionSource.SetResult(handledCount);
                }
                catch (Exception ex)
                {
                    item.CompletionSource.SetException(ex);
                }
                finally
                {
                    // 释放事件上下文资源
                    if (item.Context is IDisposable disposableContext)
                    {
                        disposableContext.Dispose();
                    }
                }
            }
        }

        private async Task<int> ProcessEventItem(EventQueueItem item)
        {
            var handledCount = 0;
            var context = item.Context;
            var payloadType = item.PayloadType;

            // 使用反射获取路由键
            var routeKeyProperty = context.GetType().GetProperty("RouteKey");
            var routeKey = routeKeyProperty?.GetValue(context)?.ToString() ?? "";

            // 直接使用继承的 protected 方法
            var matchingRoutes = GetMatchingRoutes(payloadType, routeKey);

            var tasks = new List<Task>();

            // 并行执行所有匹配的处理器
            foreach (var task in matchingRoutes.Select(route => InvokeHandlerAsync(route.Handler, context, item.AsyncExceptionHandler)))
            {
                tasks.Add(task);
                handledCount++;
            }

            // 等待所有处理器完成
            await Task.WhenAll(tasks);

            return handledCount;
        }

        private static async Task InvokeHandlerAsync(Delegate handler, object context, AsyncExceptionHandler exceptionHandler)
        {
            try
            {
                var result = handler.DynamicInvoke(context);
                if (result is Task task)
                {
                    await task;
                }
            }
            catch (Exception ex)
            {
                // 如果有异常处理回调，则调用它
                if (exceptionHandler?.Invoke(context, ex, handler) == true)
                {
                    // 重新抛出异常
                    throw;
                }
                // 否则记录日志但不抛出异常
                Console.WriteLine($"异步事件处理器执行异常: {ex.Message}");
            }
        }


        #endregion


        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            // 停止接受新事件
            _writer.Complete();

            // 取消处理任务
            _cancellationTokenSource.Cancel();

            try
            {
                // 等待处理任务完成
                _processingTask.Wait(TimeSpan.FromSeconds(5));
            }
            catch (AggregateException)
            {
                // 忽略取消异常
            }

            _cancellationTokenSource.Dispose();
        }
    }
}
