using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
    /// </summary>
    public class AsyncBus : IDisposable
    {
        /// <summary>
        /// 异步事件处理委托
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="context">事件上下文</param>
        public delegate Task AsyncEventHandler<T>(EventContext<T> context);

        /// <summary>
        /// 路由信息
        /// </summary>
        private class RouteInfo
        {
            public Type PayloadType { get; set; }
            public string Pattern { get; set; }
            public Delegate Handler { get; set; }
            public bool IsRegex { get; set; }
            public int SubscriptionOrder { get; set; }
            public string SubscriptionId { get; set; } = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// 事件队列项
        /// </summary>
        private class EventQueueItem
        {
            public object Context { get; set; }
            public Type PayloadType { get; set; }
            public TaskCompletionSource<int> CompletionSource { get; set; }
        }

        private readonly ConcurrentDictionary<string, List<RouteInfo>> _routes =
            new ConcurrentDictionary<string, List<RouteInfo>>();

        private readonly object _lockObject = new object();
        private int _globalSubscriptionCounter;
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

        #region 订阅方法

        /// <summary>
        /// 订阅事件（基于事件负载类型）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="handler">异步事件处理器</param>
        /// <returns>订阅ID，可用于取消订阅</returns>
        public string Subscribe<T>(AsyncEventHandler<T> handler)
        {
            return Subscribe(typeof(T), "*", handler, false);
        }

        /// <summary>
        /// 订阅事件（基于路由键精确匹配）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="routeKey">路由键</param>
        /// <param name="handler">异步事件处理器</param>
        /// <returns>订阅ID，可用于取消订阅</returns>
        public string Subscribe<T>(string routeKey, AsyncEventHandler<T> handler)
        {
            return Subscribe(typeof(T), routeKey, handler, false);
        }

        /// <summary>
        /// 订阅事件（基于路由键模式匹配）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="routePattern">路由模式（支持通配符*和?，或正则表达式）</param>
        /// <param name="handler">异步事件处理器</param>
        /// <param name="isRegex">是否使用正则表达式匹配</param>
        /// <returns>订阅ID，可用于取消订阅</returns>
        public string Subscribe<T>(string routePattern, AsyncEventHandler<T> handler, bool isRegex)
        {
            return Subscribe(typeof(T), routePattern, handler, isRegex);
        }

        private string Subscribe(Type payloadType, string routePattern, Delegate handler, bool isRegex)
        {
            if (payloadType == null) throw new ArgumentNullException(nameof(payloadType));
            if (string.IsNullOrWhiteSpace(routePattern)) throw new ArgumentNullException(nameof(routePattern));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var routeInfo = new RouteInfo
            {
                PayloadType = payloadType,
                Pattern = routePattern,
                Handler = handler,
                IsRegex = isRegex,
                SubscriptionOrder = Interlocked.Increment(ref _globalSubscriptionCounter)
            };

            lock (_lockObject)
            {
                var key = payloadType.FullName;
                if (key != null && !_routes.ContainsKey(key))
                {
                    _routes[key] = new List<RouteInfo>();
                }

                if (key == null) return routeInfo.SubscriptionId;
                _routes[key].Add(routeInfo);
                _routes[key] = _routes[key].OrderBy(r => r.SubscriptionOrder).ToList();
            }

            return routeInfo.SubscriptionId;
        }

        #endregion

        #region 发布方法

        /// <summary>
        /// 异步发布事件
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="payload">事件负载</param>
        /// <param name="routeKey">路由键</param>
        /// <returns>处理该事件的处理器数量</returns>
        public async Task<int> PublishAsync<T>(T payload, string routeKey = "")
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (_disposed) throw new ObjectDisposedException(nameof(AsyncBus));

            var context = new EventContext<T>(payload, routeKey);
            var completionSource = new TaskCompletionSource<int>();

            var queueItem = new EventQueueItem
            {
                Context = context,
                PayloadType = typeof(T),
                CompletionSource = completionSource
            };

            await _writer.WriteAsync(queueItem, _cancellationTokenSource.Token);
            return await completionSource.Task;
        }

        /// <summary>
        /// 同步发布事件（非阻塞）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="payload">事件负载</param>
        /// <param name="routeKey">路由键</param>
        /// <returns>是否成功加入队列</returns>
        public bool Publish<T>(T payload, string routeKey = "")
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (_disposed) return false;

            var context = new EventContext<T>(payload, routeKey);
            var completionSource = new TaskCompletionSource<int>();

            var queueItem = new EventQueueItem
            {
                Context = context,
                PayloadType = typeof(T),
                CompletionSource = completionSource
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

            // 获取匹配的路由
            var matchingRoutes = GetMatchingRoutes(payloadType, routeKey);

            var tasks = new List<Task>();

            // 并行执行所有匹配的处理器
            foreach (var task in matchingRoutes.Select(route => InvokeHandlerAsync(route.Handler, context)))
            {
                tasks.Add(task);
                handledCount++;
            }

            // 等待所有处理器完成
            await Task.WhenAll(tasks);

            return handledCount;
        }

        private static async Task InvokeHandlerAsync(Delegate handler, object context)
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
                // 可以添加日志记录
                Console.WriteLine($"异步事件处理器执行异常: {ex.Message}");
            }
        }

        #endregion

        #region 路由匹配

        private List<RouteInfo> GetMatchingRoutes(Type payloadType, string routeKey)
        {
            var matchingRoutes = new List<RouteInfo>();

            lock (_lockObject)
            {
                var typesToCheck = GetPayloadTypeHierarchy(payloadType);

                matchingRoutes.AddRange(from type in typesToCheck
                    select type.FullName
                    into key
                    where _routes.ContainsKey(key)
                    from route in _routes[key]
                    where IsRouteMatch(route.Pattern, routeKey, route.IsRegex)
                    select route);
            }

            return matchingRoutes.OrderBy(r => r.SubscriptionOrder).ToList();
        }

        private static IEnumerable<Type> GetPayloadTypeHierarchy(Type payloadType)
        {
            var types = new List<Type> { payloadType };

            var baseType = payloadType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                types.Add(baseType);
                baseType = baseType.BaseType;
            }

            var interfaces = payloadType.GetInterfaces();
            types.AddRange(interfaces);

            return types;
        }

        private static bool IsRouteMatch(string pattern, string routeKey, bool isRegex)
        {
            if (pattern == "*") return true;

            if (isRegex)
            {
                try
                {
                    return Regex.IsMatch(routeKey, pattern);
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return WildcardMatch(pattern, routeKey);
            }
        }

        private static bool WildcardMatch(string pattern, string text)
        {
            if (string.IsNullOrEmpty(pattern) && string.IsNullOrEmpty(text))
                return true;

            if (string.IsNullOrEmpty(pattern))
                return false;

            if (string.IsNullOrEmpty(text))
                return pattern == "*";

            var regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            return Regex.IsMatch(text, regexPattern);
        }

        #endregion

        #region 取消订阅

        public bool Unsubscribe(string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId)) return false;

            lock (_lockObject)
            {
                foreach (var kvp in _routes)
                {
                    var routesToRemove = kvp.Value.Where(r => r.SubscriptionId == subscriptionId).ToList();
                    if (routesToRemove.Any())
                    {
                        foreach (var route in routesToRemove)
                        {
                            kvp.Value.Remove(route);
                        }

                        if (kvp.Value.Count == 0)
                        {
                            _routes.TryRemove(kvp.Key, out _);
                        }

                        return true;
                    }
                }
            }

            return false;
        }

        public int UnsubscribeAll<T>()
        {
            lock (_lockObject)
            {
                var key = typeof(T).FullName;
                if (key == null || !_routes.TryGetValue(key, out var route)) return 0;
                var count = route.Count;
                _routes.TryRemove(key, out _);
                return count;
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