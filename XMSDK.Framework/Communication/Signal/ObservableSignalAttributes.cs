using System;

namespace XMSDK.Framework.Communication.Signal
{
    /// <summary>
    /// 标记类中包含可观察的信号集合，用于支持外部访问和反射等功能。
    /// </summary>
    /// <remarks>
    /// 应用此特性的类通常继承自 <see cref="PollingSignalHost"/>，
    /// 并包含一个或多个使用 <see cref="ObservableSignalAttribute"/> 标记的属性。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public sealed class ObservableSignalCollectionAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="ObservableSignalCollectionAttribute"/> 类的新实例。
        /// </summary>
        public ObservableSignalCollectionAttribute()
        {
        }

        /// <summary>
        /// 获取或设置信号集合的描述信息。
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 获取或设置信号集合的分组名称。
        /// </summary>
        public string? Group { get; set; }
    }

    /// <summary>
    /// 标记可观察的信号属性，提供信号的元数据信息。
    /// </summary>
    /// <remarks>
    /// 此特性用于标记继承自 <see cref="PollingSignalHost"/> 的类中的属性，
    /// 这些属性通常对应底层的信号地址，并提供类型安全的访问接口。
    /// </remarks>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public sealed class ObservableSignalAttribute : Attribute
    {
        /// <summary>
        /// 初始化 <see cref="ObservableSignalAttribute"/> 类的新实例。
        /// </summary>
        public ObservableSignalAttribute()
        {
        }

        /// <summary>
        /// 获取或设置信号的显示名称。
        /// </summary>
        /// <value>信号的人可读显示名称，用于UI展示或日志记录。</value>
        public string? Name { get; set; }

        /// <summary>
        /// 获取或设置信号的底层地址。
        /// </summary>
        /// <value>
        /// 信号在底层系统中的唯一标识符，通常对应 PLC、设备或其他数据源中的具体地址。
        /// </value>
        public string? Address { get; set; }

        /// <summary>
        /// 获取或设置信号值的数据类型。
        /// </summary>
        /// <value>信号值的 .NET 类型，如 typeof(bool)、typeof(int) 等。</value>
        public Type? Type { get; set; }

        /// <summary>
        /// 获取或设置信号的描述信息。
        /// </summary>
        /// <value>信号的详细描述，用于文档生成或工具提示。</value>
        public string? Description { get; set; }

        /// <summary>
        /// 获取或设置信号的分组名称。
        /// </summary>
        /// <value>用于对相关信号进行逻辑分组，便于管理和展示。</value>
        public string? Group { get; set; }

        /// <summary>
        /// 获取或设置信号是否为只读。
        /// </summary>
        /// <value>
        /// 如果为 <c>true</c>，表示信号只能读取不能写入；
        /// 如果为 <c>false</c>，表示信号可读写。默认为 <c>false</c>。
        /// </value>
        public bool IsReadOnly { get; set; } = false;

        /// <summary>
        /// 获取或设置信号的单位。
        /// </summary>
        /// <value>信号值的物理单位，如 "V"、"A"、"℃" 等。</value>
        public string? Unit { get; set; }

        /// <summary>
        /// 获取或设置信号的最小值。
        /// </summary>
        /// <value>用于数值类型信号的范围限制和验证。</value>
        public object? MinValue { get; set; }

        /// <summary>
        /// 获取或设置信号的最大值。
        /// </summary>
        /// <value>用于数值类型信号的范围限制和验证。</value>
        public object? MaxValue { get; set; }

        /// <summary>
        /// 获取或设置信号的默认值。
        /// </summary>
        /// <value>信号的初始或默认值。</value>
        public object? DefaultValue { get; set; }

        /// <summary>
        /// 获取或设置信号的格式化字符串。
        /// </summary>
        /// <value>用于格式化信号值的显示，如数值精度、日期时间格式等。</value>
        public string? Format { get; set; }
    }
}
