using System;
using Newtonsoft.Json;

namespace XMSDK.Framework.Communication.SimpleTCP;

public static class MessageProtocol
{
    public static string EscapeMultiLine(string text)
    {
        return string.IsNullOrEmpty(text) ? text : text.Replace("\r\n", "\\n").Replace("\n", "\\n").Replace("\r", "\\n");
    }

    public static string UnescapeMultiLine(string text)
    {
        return string.IsNullOrEmpty(text) ? text : text.Replace("\\n", Environment.NewLine);
    }

    public static string FormatSignalMessage(string signalName, object? value)
    {
        string valueStr;
        if (value is string stringValue)
        {
            valueStr = $"\"{EscapeMultiLine(stringValue)}\"";
        }
        else
        {
            valueStr = value?.ToString() ?? "null";
        }
            
        return $"${signalName}={valueStr}";
    }

    public static string FormatCommandMessage(string commandName)
    {
        return $"#{commandName}";
    }

    public static bool TryParseSignalMessage(string message, out string? signalName, out string? value)
    {
        signalName = null;
        value = null;

        if (!message.StartsWith("$"))
            return false;

        var equalIndex = message.IndexOf('=');
        if (equalIndex == -1)
            return false;

        signalName = message.Substring(1, equalIndex - 1);
        value = message.Substring(equalIndex + 1);

        // 处理字符串值的引号
        if (value.StartsWith("\"") && value.EndsWith("\"") && value.Length >= 2)
        {
            value = UnescapeMultiLine(value.Substring(1, value.Length - 2));
        }

        return true;
    }

    public static bool TryParseCommandMessage(string message, out string? commandName)
    {
        commandName = null;

        if (!message.StartsWith("#"))
            return false;

        commandName = message.Substring(1);
        return !string.IsNullOrEmpty(commandName);
    }

    public static T? ConvertValue<T>(string value)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)value;

        if (typeof(T) == typeof(bool))
            return (T)(object)bool.Parse(value);

        if (typeof(T) == typeof(int))
            return (T)(object)int.Parse(value);

        if (typeof(T) == typeof(double))
            return (T)(object)double.Parse(value);

        if (typeof(T) == typeof(float))
            return (T)(object)float.Parse(value);

        // 对于复杂类型，使用JSON反序列化
        return JsonConvert.DeserializeObject<T>(value);
    }

    public static object? ConvertValue(string value, Type targetType)
    {
        if (targetType == typeof(string))
            return value;

        if (targetType == typeof(bool))
            return bool.Parse(value);

        if (targetType == typeof(int))
            return int.Parse(value);

        if (targetType == typeof(double))
            return double.Parse(value);

        if (targetType == typeof(float))
            return float.Parse(value);

        // 对于复杂类型，使用JSON反序列化
        return JsonConvert.DeserializeObject(value, targetType);
    }
}