using System;
using System.IO;
using NUnit.Framework;
using XMSDK.Framework.Config;

namespace XMSDK.Tests
{
    // 未加特性的静态类
    public static class NoBindStaticClass
    {
        public static string Name { get; set; } = "default";
    }

    // 加了 BindLocalFileAttribute 的静态类
    [BindLocalFile("test_config1.json")]
    public static class BindStaticClass
    {
        public static int Value { get; set; } = 42;
    }

    // 带嵌套静态类
    [BindLocalFile("test_config2.json")]
    public static class OuterStaticClass
    {
        public static string OuterProp { get; set; } = "outer";
        public static class InnerStaticClass
        {
            public static bool Flag { get; set; } = true;
        }
    }

    [TestFixture]
    public class TestLocalFileBinder
    {
        private string GetFilePath(string fileName) => Path.Combine(AppContext.BaseDirectory, fileName);

        [SetUp]
        public void Cleanup()
        {
            // 清理测试文件
            var files = new[] { "test_config1.json", "test_config2.json" };
            foreach (var file in files)
            {
                var path = GetFilePath(file);
                if (File.Exists(path)) File.Delete(path);
            }
        }

        [Test]
        public void Should_Throw_When_NoBindAttribute()
        {
            Assert.Throws<ArgumentException>(() => LocalFileBinder.Bind(typeof(NoBindStaticClass)));
        }

        [Test]
        public void Should_Bind_And_CreateFile()
        {
            LocalFileBinder.Bind(typeof(BindStaticClass));
            var path = GetFilePath("test_config1.json");
            Assert.IsTrue(File.Exists(path));
            // 检查属性值是否为默认值
            Assert.AreEqual(42, BindStaticClass.Value);
        }

        [Test]
        public void Should_Bind_NestedStaticClass()
        {
            LocalFileBinder.Bind(typeof(OuterStaticClass));
            var path = GetFilePath("test_config2.json");
            Assert.IsTrue(File.Exists(path));
            Assert.AreEqual("outer", OuterStaticClass.OuterProp);
            Assert.AreEqual(true, OuterStaticClass.InnerStaticClass.Flag);
        }

        [Test]
        public void Should_Save_And_Load()
        {
            // 修改属性值
            BindStaticClass.Value = 99;
            LocalFileBinder.Bind(typeof(BindStaticClass));
            LocalFileBinder.Save(typeof(BindStaticClass));
            // 重置属性值
            BindStaticClass.Value = 0;
            // 重新加载
            LocalFileBinder.Bind(typeof(BindStaticClass));
            Assert.AreEqual(99, BindStaticClass.Value);
        }

        [Test]
        public void Should_SaveAll()
        {
            LocalFileBinder.Bind(typeof(BindStaticClass));
            LocalFileBinder.Bind(typeof(OuterStaticClass));
            BindStaticClass.Value = 123;
            OuterStaticClass.OuterProp = "changed";
            OuterStaticClass.InnerStaticClass.Flag = false;
            LocalFileBinder.SaveAll();
            // 重置
            BindStaticClass.Value = 0;
            OuterStaticClass.OuterProp = "";
            OuterStaticClass.InnerStaticClass.Flag = true;
            // 重新加载
            LocalFileBinder.Bind(typeof(BindStaticClass));
            LocalFileBinder.Bind(typeof(OuterStaticClass));
            Assert.AreEqual(123, BindStaticClass.Value);
            Assert.AreEqual("changed", OuterStaticClass.OuterProp);
            Assert.AreEqual(false, OuterStaticClass.InnerStaticClass.Flag);
        }
    }
}

