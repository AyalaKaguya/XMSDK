using System;

namespace XMSDK.Framework.EventBus
{
    /// <summary>
    /// 事件上下文，包装任何类型的事件负载
    /// </summary>
    /// <typeparam name="T">事件负载类型</typeparam>
    public class EventContext<T> : IDisposable
    {
        private bool _disposed;

        /// <summary>
        /// 事件负载实例
        /// </summary>
        public T Payload { get; private set; }

        /// <summary>
        /// 事件路由键，用于路由决策
        /// </summary>
        public string RouteKey { get; set; }

        /// <summary>
        /// 事件ID，唯一标识
        /// </summary>
        public string EventId { get; private set; }

        /// <summary>
        /// 事件创建时间
        /// </summary>
        public DateTime CreatedAt { get; private set; }

        /// <summary>
        /// 是否继续订阅标记（用于一次性订阅控制）
        /// </summary>
        public bool ContinueSubscription { get; set; } = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="payload">事件负载</param>
        /// <param name="routeKey">路由键</param>
        public EventContext(T payload, string routeKey = "")
        {
            Payload = payload;
            RouteKey = routeKey ?? "";
            EventId = Guid.NewGuid().ToString();
            CreatedAt = DateTime.Now;
        }

        /// <summary>
        /// 停止继续订阅（用于一次性订阅）
        /// </summary>
        public void StopSubscription()
        {
            ContinueSubscription = false;
        }

        /// <summary>
        /// 释放资源，由事件发布者负责
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                // 如果负载实现了IDisposable，则释放它
                if (Payload is IDisposable disposablePayload)
                {
                    disposablePayload.Dispose();
                }

                _disposed = true;
            }
        }
    }
}
