using System;

namespace XMSDK.Framework.Communication.Signal;

/// <summary>
/// 信号信息结构，包含信号的完整元数据。
/// </summary>
public class SignalInfo
{
    /// <summary>
    /// 信号地址
    /// </summary>
    public string Address { get; set; } = string.Empty;

    /// <summary>
    /// 信号显示名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 信号描述
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 信号类型
    /// </summary>
    public Type SignalType { get; set; } = typeof(object);

    /// <summary>
    /// 当前信号值
    /// </summary>
    public object? CurrentValue { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.Now;

    /// <summary>
    /// 是否只读
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string Group { get; set; } = string.Empty;

    /// <summary>
    /// 单位
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// 格式化字符串
    /// </summary>
    public string Format { get; set; } = string.Empty;
}