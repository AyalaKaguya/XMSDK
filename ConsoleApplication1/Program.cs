using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using XMSDK.Framework.Logger;
using XMSDK.Framework.Config;

namespace ConsoleApplication1
{
    [BindLocalFile("test-config.json")]
    public static class TestConfig
    {
        public static string DatabaseUrl { get; set; } = "localhost:1433";
        public static int MaxConnections { get; set; } = 100;
        public static bool EnableLogging { get; set; } = true;
        
        public static class Advanced
        {
            public static int Timeout { get; set; } = 30;
            public static string Protocol { get; set; } = "TCP";
        }
    }

    internal class Program
    {
        public static void Main(string[] args)
        {
            // 触发配置绑定
            LocalFileBinder.BindAll();
            
            Console.WriteLine("=== 初始配置值 ===");
            Console.WriteLine($"DatabaseUrl: {TestConfig.DatabaseUrl}");
            Console.WriteLine($"MaxConnections: {TestConfig.MaxConnections}");
            Console.WriteLine($"EnableLogging: {TestConfig.EnableLogging}");
            Console.WriteLine($"Advanced.Timeout: {TestConfig.Advanced.Timeout}");
            Console.WriteLine($"Advanced.Protocol: {TestConfig.Advanced.Protocol}");

            Console.WriteLine("\n=== 修改配置值 ===");
            TestConfig.MaxConnections = 700;
            TestConfig.DatabaseUrl = "newserver:5432";
            TestConfig.Advanced.Timeout = 60;
            
            Console.WriteLine($"修改后 MaxConnections: {TestConfig.MaxConnections}");
            Console.WriteLine($"修改后 DatabaseUrl: {TestConfig.DatabaseUrl}");
            Console.WriteLine($"修改后 Advanced.Timeout: {TestConfig.Advanced.Timeout}");
            
            Console.WriteLine("\n=== 保存配置到文件 ===");
            LocalFileBinder.Save(typeof(TestConfig));
            
            
            using (new TraceMessageReceiver()
                       // .AddToDebug()
                       .AddToTrace()
                       .AddProcesser(new LogToConsoleProcesser())
                       .AddProcesser(new LogToFolderProcesser(@"D:\logs")))
            {
                Trace.WriteLine("This is a test message for Trace.");
                Debug.WriteLine("This is a test message for Debug.");
            }
        }
    }
}