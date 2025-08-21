using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace XMSDK.Framework.EventBus
{
    /// <summary>
    /// 直接事件总线，
    /// 调用者直接调用事件处理方法，
    /// 不经过异步队列或其他处理。
    /// 支持路由功能，可基于路由键进行事件分发。
    /// 支持多对多订阅模式，按订阅顺序执行。
    /// </summary>
    public class DirectBus
    {
        /// <summary>
        /// 事件处理委托
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="context">事件上下文</param>
        public delegate void EventHandler<T>(EventContext<T> context);

        /// <summary>
        /// 路由信息
        /// </summary>
        private class RouteInfo
        {
            public Type PayloadType { get; set; }
            public string Pattern { get; set; }
            public Delegate Handler { get; set; }
            public bool IsRegex { get; set; }
            public int SubscriptionOrder { get; set; } // 订阅顺序，替代优先级
            public string SubscriptionId { get; set; } = Guid.NewGuid().ToString(); // 订阅唯一标识
        }

        private readonly ConcurrentDictionary<string, List<RouteInfo>> _routes =
            new ConcurrentDictionary<string, List<RouteInfo>>();

        private readonly object _lockObject = new object();
        private int _globalSubscriptionCounter; // 全局订阅计数器，用于确定订阅顺序

        /// <summary>
        /// 订阅事件（基于事件负载类型）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="handler">事件处理器</param>
        /// <returns>订阅ID，可用于取消订阅</returns>
        public string Subscribe<T>(EventHandler<T> handler)
        {
            return Subscribe(typeof(T), "*", handler, false);
        }

        /// <summary>
        /// 订阅事件（基于路由键精确匹配）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="routeKey">路由键</param>
        /// <param name="handler">事件处理器</param>
        /// <returns>订阅ID，可用于取消订阅</returns>
        public string Subscribe<T>(string routeKey, EventHandler<T> handler)
        {
            return Subscribe(typeof(T), routeKey, handler, false);
        }

        /// <summary>
        /// 订阅事件（基于路由键模式匹配）
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="routePattern">路由模式（支持通配符*和?，或正则表达式）</param>
        /// <param name="handler">事件处理器</param>
        /// <param name="isRegex">是否使用正则表达式匹配</param>
        /// <returns>订阅ID，可用于取消订阅</returns>
        public string Subscribe<T>(string routePattern, EventHandler<T> handler, bool isRegex)
        {
            return Subscribe(typeof(T), routePattern, handler, isRegex);
        }

        /// <summary>
        /// 通用订阅方法
        /// </summary>
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

                if (key != null)
                {
                    _routes[key].Add(routeInfo);
                    // 按订阅顺序排序
                    _routes[key] = _routes[key].OrderBy(r => r.SubscriptionOrder).ToList();
                }
            }

            return routeInfo.SubscriptionId;
        }

        /// <summary>
        /// 发布事件
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="payload">事件负载</param>
        /// <param name="routeKey">路由键</param>
        /// <returns>处理该事件的处理器数量</returns>
        public int Publish<T>(T payload, string routeKey = "")
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));

            var context = new EventContext<T>(payload, routeKey);

            try
            {
                return PublishContext(context);
            }
            finally
            {
                // 由事件发布者负责释放资源
                context.Dispose();
            }
        }

        /// <summary>
        /// 发布事件上下文
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <param name="context">事件上下文</param>
        /// <returns>处理该事件的处理器数量</returns>
        private int PublishContext<T>(EventContext<T> context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var handledCount = 0;
            var payloadType = typeof(T);
            var routeKey = context.RouteKey ?? "";

            // 获取匹配的路由（按订阅顺序）
            var matchingRoutes = GetMatchingRoutes(payloadType, routeKey);

            var routesToRemove = new List<RouteInfo>();

            // 按订阅顺序执行每个处理器
            foreach (var route in matchingRoutes)
            {
                try
                {
                    // 多对多订阅，直接调用
                    route.Handler.DynamicInvoke(context);
                    handledCount++;
                    // 检查是否需要停止订阅
                    if (context.ContinueSubscription) continue;
                    routesToRemove.Add(route);
                    // 重置标记，以便其他订阅者继续处理
                    context.ContinueSubscription = true;
                }
                catch (Exception ex)
                {
                    // 可以添加日志记录
                    Console.WriteLine($"事件处理器执行异常: {ex.Message}");
                }
            }

            // 移除标记为需要移除的路由
            RemoveRoutes(routesToRemove);

            return handledCount;
        }

        /// <summary>
        /// 获取匹配的路由（按订阅顺序）
        /// </summary>
        private List<RouteInfo> GetMatchingRoutes(Type payloadType, string routeKey)
        {
            var matchingRoutes = new List<RouteInfo>();

            lock (_lockObject)
            {
                // 检查当前类型及其基类和接口
                var typesToCheck = GetPayloadTypeHierarchy(payloadType);

                foreach (var type in typesToCheck)
                {
                    var key = type.FullName;
                    if (key != null && _routes.TryGetValue(key, out var route1))
                    {
                        matchingRoutes.AddRange(route1.Where(route =>
                            IsRouteMatch(route.Pattern, routeKey, route.IsRegex)));
                    }
                }
            }

            // 按订阅顺序排序，确保按注册顺序执行
            return matchingRoutes.OrderBy(r => r.SubscriptionOrder).ToList();
        }

        /// <summary>
        /// 获取负载类型层次结构
        /// </summary>
        private static IEnumerable<Type> GetPayloadTypeHierarchy(Type payloadType)
        {
            var types = new List<Type> { payloadType };

            // 添加基类
            var baseType = payloadType.BaseType;
            while (baseType != null && baseType != typeof(object))
            {
                types.Add(baseType);
                baseType = baseType.BaseType;
            }

            // 添加接口
            var interfaces = payloadType.GetInterfaces();
            types.AddRange(interfaces);

            return types;
        }

        /// <summary>
        /// 检查路由是否匹配
        /// </summary>
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
                // 支持通配符匹配
                return WildcardMatch(pattern, routeKey);
            }
        }

        /// <summary>
        /// 通配符匹配
        /// </summary>
        private static bool WildcardMatch(string pattern, string text)
        {
            if (string.IsNullOrEmpty(pattern) && string.IsNullOrEmpty(text))
                return true;

            if (string.IsNullOrEmpty(pattern))
                return false;

            if (string.IsNullOrEmpty(text))
                return pattern == "*";

            // 转换通配符模式为正则表达式
            var regexPattern = "^" + Regex.Escape(pattern)
                .Replace("\\*", ".*")
                .Replace("\\?", ".") + "$";

            return Regex.IsMatch(text, regexPattern);
        }

        /// <summary>
        /// 移除路由
        /// </summary>
        private void RemoveRoutes(List<RouteInfo> routesToRemove)
        {
            if (routesToRemove.Count == 0) return;

            lock (_lockObject)
            {
                foreach (var routeToRemove in routesToRemove)
                {
                    var key = routeToRemove.PayloadType.FullName;
                    if (key == null || !_routes.TryGetValue(key, out var route)) continue;
                    route.Remove(routeToRemove);
                    if (_routes[key].Count == 0)
                    {
                        _routes.TryRemove(key, out _);
                    }
                }
            }
        }

        /// <summary>
        /// 根据订阅ID取消订阅
        /// </summary>
        /// <param name="subscriptionId">订阅ID</param>
        /// <returns>是否成功取消订阅</returns>
        public bool Unsubscribe(string subscriptionId)
        {
            if (string.IsNullOrEmpty(subscriptionId)) return false;

            lock (_lockObject)
            {
                foreach (var kvp in _routes)
                {
                    var routesToRemove = kvp.Value.Where(r => r.SubscriptionId == subscriptionId).ToList();
                    if (!routesToRemove.Any()) continue;
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

            return false;
        }

        /// <summary>
        /// 取消订阅指定类型的所有事件
        /// </summary>
        /// <typeparam name="T">事件负载类型</typeparam>
        /// <returns>取消的订阅数量</returns>
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
        
    }
}