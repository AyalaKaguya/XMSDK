using System;

namespace XMSDK.Framework.Config;

[AttributeUsage(AttributeTargets.Property)]
public class DefaultValueAttribute(object value) : Attribute
{
    public object Value { get; } = value;
}