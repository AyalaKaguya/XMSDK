using System;
using System.Diagnostics;
using System.Threading.Tasks;
using XMSDK.Framework.Communication;
using XMSDK.Framework.Logger;
using XMSDK.Framework.Config;
using XMSDK.Framework.Crypt;
using XMSDK.Framework.EventBus;

namespace ConsoleApplication1
{
    [BindLocalFile("test-config.json")]
    public static class TestConfig
    {
        public static string DatabaseUrl { get; set; } = "localhost:1433";
        public static int MaxConnections { get; set; } = 100;
        public static bool EnableLogging { get; set; } = true;
        public static int ServerPort { get; set; } = 8088;

        public static class Advanced
        {
            public static int Timeout { get; set; } = 30;
            public static string Protocol { get; set; } = "TCP";
        }
    }

    internal static class Program
    {
        public static void Main(string[] args)
        {
            // // 测试日志记录
            // Console.WriteLine("\n=== 测试 Trace 和 Debug 输出 ===");
            // new TraceMessageReceiver()
            //     // .AddToDebug() // 只输出到Debug
            //     .AddToTrace() // 同时输出到Trace和Debug
            //     .AddProcesser(new LogToConsoleProcesser())
            //     .AddProcesser(new LogToFolderProcesser(@"D:\logs"));
            //
            // Trace.WriteLine("This is a test message for Trace.");
            // Debug.WriteLine("This is a test message for Debug."); // Release模式下不会输出
            //
            //
            // TransmissionTest.TestTransmissionCreate();
            // TransmissionTest.TestSignalTransmission();
            // TransmissionTest.TestCommandTransmission();
            
            
            Console.WriteLine("=== 测试事件队列 ===");

            var bus = new DirectBus();
            
            bus.Subscribe<string>("string", (context) =>
            {
                Console.WriteLine($"接收到字符串事件：{context.Payload}");
            });
            
            bus.Publish("Hello EventBus!","string");
            
            Console.WriteLine("=== 测试完成 ===");
        }
        
        
    }
}