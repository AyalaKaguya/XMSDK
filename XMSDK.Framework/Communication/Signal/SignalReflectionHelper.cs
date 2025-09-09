using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace XMSDK.Framework.Communication.Signal
{
    /// <summary>
    /// 信号反射工具，用于从标记了 ObservableSignal 特性的类中提取信号信息。
    /// </summary>
    public static class SignalReflectionHelper
    {
        /// <summary>
        /// 从指定对象中提取所有可观察的信号信息
        /// </summary>
        /// <param name="target">目标对象，应标记有 ObservableSignalCollection 特性</param>
        /// <returns>信号信息列表</returns>
        public static List<SignalInfo> ExtractSignals(object? target)
        {
            if (target == null) return new List<SignalInfo>();

            var type = target.GetType();
            var signals = new List<SignalInfo>();

            // 检查类是否标记了 ObservableSignalCollection 特性
            var collectionAttr = type.GetCustomAttribute<ObservableSignalCollectionAttribute>();
            if (collectionAttr == null)
            {
                return signals;
            }

            // 获取所有标记了 ObservableSignal 特性的属性
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<ObservableSignalAttribute>() != null);

            foreach (var property in properties)
            {
                var signalAttr = property.GetCustomAttribute<ObservableSignalAttribute>()!;
                
                var signalInfo = new SignalInfo
                {
                    Address = signalAttr.Address ?? property.Name,
                    Name = signalAttr.Name ?? property.Name,
                    Description = signalAttr.Description ?? string.Empty,
                    SignalType = signalAttr.Type ?? property.PropertyType,
                    IsReadOnly = signalAttr.IsReadOnly || !property.CanWrite,
                    Group = signalAttr.Group ?? string.Empty,
                    Unit = signalAttr.Unit ?? string.Empty,
                    Format = signalAttr.Format ?? string.Empty,
                    LastUpdated = DateTime.Now
                };

                // 尝试获取当前值
                try
                {
                    if (property.CanRead)
                    {
                        signalInfo.CurrentValue = property.GetValue(target);
                    }
                }
                catch
                {
                    signalInfo.CurrentValue = null;
                }

                signals.Add(signalInfo);
            }

            return signals;
        }

        /// <summary>
        /// 更新指定信号的值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="address">信号地址</param>
        /// <param name="newValue">新值</param>
        /// <returns>是否更新成功</returns>
        public static bool UpdateSignalValue(object? target, string? address, object? newValue)
        {
            if (target == null || string.IsNullOrEmpty(address)) return false;

            var type = target.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<ObservableSignalAttribute>() != null);

            foreach (var property in properties)
            {
                var signalAttr = property.GetCustomAttribute<ObservableSignalAttribute>()!;
                var signalAddress = signalAttr.Address ?? property.Name;

                if (signalAddress == address && property.CanWrite && !signalAttr.IsReadOnly)
                {
                    try
                    {
                        // 类型转换
                        var convertedValue = ConvertValue(newValue, property.PropertyType);
                        property.SetValue(target, convertedValue);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 获取指定信号的当前值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="address">信号地址</param>
        /// <returns>信号值，如果不存在返回null</returns>
        public static object? GetSignalValue(object? target, string? address)
        {
            if (target == null || string.IsNullOrEmpty(address)) return null;

            var type = target.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.GetCustomAttribute<ObservableSignalAttribute>() != null);

            foreach (var property in properties)
            {
                var signalAttr = property.GetCustomAttribute<ObservableSignalAttribute>()!;
                var signalAddress = signalAttr.Address ?? property.Name;

                if (signalAddress == address && property.CanRead)
                {
                    try
                    {
                        return property.GetValue(target);
                    }
                    catch
                    {
                        return null;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// 刷新所有信号的值
        /// </summary>
        /// <param name="target">目标对象</param>
        /// <param name="signals">信号信息列表</param>
        public static void RefreshSignalValues(object? target, List<SignalInfo>? signals)
        {
            if (target == null || signals == null) return;

            foreach (var signal in signals)
            {
                var newValue = GetSignalValue(target, signal.Address);
                if (newValue != null || signal.CurrentValue != null)
                {
                    signal.CurrentValue = newValue;
                    signal.LastUpdated = DateTime.Now;
                }
            }
        }

        /// <summary>
        /// 值类型转换辅助方法
        /// </summary>
        /// <param name="value">原始值</param>
        /// <param name="targetType">目标类型</param>
        /// <returns>转换后的值</returns>
        private static object? ConvertValue(object? value, Type targetType)
        {
            if (value == null) return null;
            if (targetType.IsAssignableFrom(value.GetType())) return value;

            // 处理可空类型
            var underlyingType = Nullable.GetUnderlyingType(targetType) ?? targetType;

            return Convert.ChangeType(value, underlyingType);
        }
    }
}
