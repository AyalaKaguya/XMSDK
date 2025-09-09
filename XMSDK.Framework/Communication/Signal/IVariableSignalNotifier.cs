using System;

namespace XMSDK.Framework.Communication.Signal
{
    /// <summary>
    /// 可变信号通知接口，提供信号值变化的事件通知。
    /// </summary>
    public interface IVariableSignalNotifier
    {
        /// <summary>
        /// 信号值变化事件。
        /// </summary>
        /// <param name="address">信号地址</param>
        /// <param name="oldValue">旧值</param>
        /// <param name="newValue">新值</param>
        /// <param name="timestamp">变化时间戳</param>
        event Action<string, object, object, DateTime>? SignalValueChanged;
    }
}
