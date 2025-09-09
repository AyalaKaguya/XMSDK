using System;
using System.Collections.Generic;
using System.Globalization;

namespace XMSDK.Framework.Communication.Signal
{
    /// <summary>
    /// 信号值转换器接口，用于将信号值转换为字符串显示或从字符串解析为值。
    /// </summary>
    public interface ISignalValueConverter
    {
        /// <summary>
        /// 支持的信号类型
        /// </summary>
        Type SupportedType { get; }

        /// <summary>
        /// 将信号值转换为显示字符串
        /// </summary>
        /// <param name="value">信号值</param>
        /// <param name="format">格式化字符串</param>
        /// <returns>显示字符串</returns>
        string ValueToString(object? value, string? format = null);

        /// <summary>
        /// 从字符串解析信号值
        /// </summary>
        /// <param name="text">输入文本</param>
        /// <param name="value">解析出的值</param>
        /// <returns>是否解析成功</returns>
        bool TryParseValue(string text, out object? value);

        /// <summary>
        /// 验证值是否有效
        /// </summary>
        /// <param name="value">要验证的值</param>
        /// <returns>是否有效</returns>
        bool IsValidValue(object? value);
    }

    /// <summary>
    /// 布尔类型信号转换器
    /// </summary>
    public class BoolSignalConverter : ISignalValueConverter
    {
        public Type SupportedType => typeof(bool);

        public string ValueToString(object? value, string? format = null)
        {
            if (value is bool boolValue)
            {
                return format switch
                {
                    "ON_OFF" => boolValue ? "ON" : "OFF",
                    "开关" => boolValue ? "开" : "关",
                    "启停" => boolValue ? "启动" : "停止",
                    _ => boolValue.ToString()
                };
            }
            return "N/A";
        }

        public bool TryParseValue(string text, out object? value)
        {
            value = null;
            text = text?.Trim().ToLower();
            
            if (string.IsNullOrEmpty(text))
                return false;

            switch (text)
            {
                case "true":
                case "1":
                case "on":
                case "开":
                case "启动":
                    value = true;
                    return true;
                case "false":
                case "0":
                case "off":
                case "关":
                case "停止":
                    value = false;
                    return true;
                default:
                    return false;
            }
        }

        public bool IsValidValue(object? value) => value is bool;
    }

    /// <summary>
    /// 整数类型信号转换器
    /// </summary>
    public class IntSignalConverter : ISignalValueConverter
    {
        public Type SupportedType => typeof(int);

        public string ValueToString(object? value, string? format = null)
        {
            if (value is int intValue)
            {
                return string.IsNullOrEmpty(format) ? intValue.ToString() : intValue.ToString(format);
            }
            return "N/A";
        }

        public bool TryParseValue(string text, out object? value)
        {
            value = null;
            if (int.TryParse(text, out int result))
            {
                value = result;
                return true;
            }
            return false;
        }

        public bool IsValidValue(object? value) => value is int;
    }

    /// <summary>
    /// 长整数类型信号转换器
    /// </summary>
    public class LongSignalConverter : ISignalValueConverter
    {
        public Type SupportedType => typeof(long);

        public string ValueToString(object? value, string? format = null)
        {
            if (value is long longValue)
            {
                return string.IsNullOrEmpty(format) ? longValue.ToString() : longValue.ToString(format);
            }
            return "N/A";
        }

        public bool TryParseValue(string text, out object? value)
        {
            value = null;
            if (long.TryParse(text, out long result))
            {
                value = result;
                return true;
            }
            return false;
        }

        public bool IsValidValue(object? value) => value is long;
    }

    /// <summary>
    /// 浮点数类型信号转换器
    /// </summary>
    public class FloatSignalConverter : ISignalValueConverter
    {
        public Type SupportedType => typeof(float);

        public string ValueToString(object? value, string? format = null)
        {
            if (value is float floatValue)
            {
                return string.IsNullOrEmpty(format) ? floatValue.ToString(CultureInfo.InvariantCulture) 
                    : floatValue.ToString(format, CultureInfo.InvariantCulture);
            }
            return "N/A";
        }

        public bool TryParseValue(string text, out object? value)
        {
            value = null;
            if (float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float result))
            {
                value = result;
                return true;
            }
            return false;
        }

        public bool IsValidValue(object? value) => value is float;
    }

    /// <summary>
    /// 双精度浮点数类型信号转换器
    /// </summary>
    public class DoubleSignalConverter : ISignalValueConverter
    {
        public Type SupportedType => typeof(double);

        public string ValueToString(object? value, string? format = null)
        {
            if (value is double doubleValue)
            {
                return string.IsNullOrEmpty(format) ? doubleValue.ToString(CultureInfo.InvariantCulture)
                    : doubleValue.ToString(format, CultureInfo.InvariantCulture);
            }
            return "N/A";
        }

        public bool TryParseValue(string text, out object? value)
        {
            value = null;
            if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
            {
                value = result;
                return true;
            }
            return false;
        }

        public bool IsValidValue(object? value) => value is double;
    }

    /// <summary>
    /// 信号值转换器管理器
    /// </summary>
    public static class SignalConverterManager
    {
        private static readonly Dictionary<Type, ISignalValueConverter> _converters = new()
        {
            { typeof(bool), new BoolSignalConverter() },
            { typeof(int), new IntSignalConverter() },
            { typeof(long), new LongSignalConverter() },
            { typeof(float), new FloatSignalConverter() },
            { typeof(double), new DoubleSignalConverter() }
        };

        /// <summary>
        /// 注册信号转换器
        /// </summary>
        /// <param name="converter">转换器实例</param>
        public static void RegisterConverter(ISignalValueConverter converter)
        {
            _converters[converter.SupportedType] = converter;
        }

        /// <summary>
        /// 获取指定类型的转换器
        /// </summary>
        /// <param name="type">信号类型</param>
        /// <returns>转换器实例，如果不存在则返回null</returns>
        public static ISignalValueConverter? GetConverter(Type type)
        {
            return _converters.TryGetValue(type, out var converter) ? converter : null;
        }

        /// <summary>
        /// 将信号值转换为显示字符串
        /// </summary>
        /// <param name="value">信号值</param>
        /// <param name="type">信号类型</param>
        /// <param name="format">格式化字符串</param>
        /// <returns>显示字符串</returns>
        public static string ConvertToString(object? value, Type type, string? format = null)
        {
            var converter = GetConverter(type);
            return converter?.ValueToString(value, format) ?? value?.ToString() ?? "N/A";
        }

        /// <summary>
        /// 从字符串解析信号值
        /// </summary>
        /// <param name="text">输入文本</param>
        /// <param name="type">目标类型</param>
        /// <param name="value">解析出的值</param>
        /// <returns>是否解析成功</returns>
        public static bool TryParseValue(string text, Type type, out object? value)
        {
            value = null;
            var converter = GetConverter(type);
            return converter?.TryParseValue(text, out value) ?? false;
        }
    }
}
