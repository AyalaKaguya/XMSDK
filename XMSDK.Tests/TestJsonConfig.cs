using System;
using System.IO;
using NUnit.Framework;
using XMSDK.Framework.Config;

namespace XMSDK.Tests;

// 未标记特性的类
public class NoAttributeConfig
{
    [DefaultValue("abc")]
    public string Name { get; set; }
}

// 标记 Serializable 的类
[Serializable]
public class SerializableConfig
{
    [DefaultValue("default-ser")]
    public string Value { get; set; }
}

[TestFixture]
public class TestJsonConfig
{
    private string GetTempFile() => Path.GetTempFileName();

    [Test]
    public void Should_Throw_When_NoAttribute()
    {
        var path = GetTempFile();
        Assert.Throws<InvalidOperationException>(() => new JsonConfig<NoAttributeConfig>(path));
        File.Delete(path);
    }

    [Test]
    public void Should_CreateConfig_WithSerializable()
    {
        var path = GetTempFile();
        var config = new JsonConfig<SerializableConfig>(path);
        Assert.AreEqual("default-ser", config.Instance.Value);
        File.Delete(path);
    }
}