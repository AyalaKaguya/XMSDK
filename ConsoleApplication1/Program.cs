using System;
using System.Configuration;
using System.Diagnostics;
using System.Threading.Tasks;
using XMSDK.Framework.Communication;
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
        public static int ServerPort { get; set; } = 8088;

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
            // 测试配置文件
            Console.WriteLine("=== 测试配置文件绑定 ===");
            LocalFileBinder.BindAll();

            Console.WriteLine("初始配置值");
            Console.WriteLine($"DatabaseUrl: {TestConfig.DatabaseUrl}");
            Console.WriteLine($"MaxConnections: {TestConfig.MaxConnections}");
            Console.WriteLine($"EnableLogging: {TestConfig.EnableLogging}");
            Console.WriteLine($"Advanced.Timeout: {TestConfig.Advanced.Timeout}");
            Console.WriteLine($"Advanced.Protocol: {TestConfig.Advanced.Protocol}");

            Console.WriteLine("\n修改配置值");
            TestConfig.MaxConnections = 700;
            TestConfig.DatabaseUrl = "newserver:5432";
            TestConfig.Advanced.Timeout = 60;

            Console.WriteLine($"修改后 MaxConnections: {TestConfig.MaxConnections}");
            Console.WriteLine($"修改后 DatabaseUrl: {TestConfig.DatabaseUrl}");
            Console.WriteLine($"修改后 Advanced.Timeout: {TestConfig.Advanced.Timeout}");

            Console.WriteLine("\n保存配置到文件");
            LocalFileBinder.Save(typeof(TestConfig));


            // 测试日志记录
            Console.WriteLine("测试 Trace 和 Debug 输出");
            new TraceMessageReceiver()
                // .AddToDebug()
                .AddToTrace()
                .AddProcesser(new LogToConsoleProcesser())
                .AddProcesser(new LogToFolderProcesser(@"D:\logs"));

            Trace.WriteLine("This is a test message for Trace.");
            Debug.WriteLine("This is a test message for Debug.");
            
            
            // 测试网络通信
            Console.WriteLine("\n=== 测试网络通信 ===");
            var serverHandle = SocketBuilder.Server("0.0.0.0", TestConfig.ServerPort)
                .Signal("D2816", false, (server, client, oldValue, newValue) =>
                {
                    Console.WriteLine($"Signal D2816 changed from {oldValue} to {newValue}");
                    server.Broadcast($"Signal D2816 changed to {newValue}");
                })
                .OnClientConnected((server, client) =>
                {
                    Console.WriteLine($"Client {client.Client.RemoteEndPoint} Connected");
                })
                .Build();
            
            serverHandle.Run();
            
            Task.Delay(1000).Wait(); // 等待服务器启动

            var clientHandle = SocketBuilder.Client("127.0.0.1", TestConfig.ServerPort)
                .Signal("D2816", false, (client, oldValue, newValue) =>
                {
                    Console.WriteLine($"Client received signal D2816 changed from {oldValue} to {newValue}");
                })
                .Build();
            
            clientHandle.Run();
            
            Task.Delay(1000).Wait(); // 等待客户端
            
            Console.WriteLine("测试信号发送");
            clientHandle.Signal("D2816", true); // 发送信号
            
            Task.Delay(1000).Wait(); // 等待信号处理
            
            Console.WriteLine("测试服务端信号发送");
            serverHandle.Signal("D2816", false);
            
            Task.Delay(1000).Wait(); // 等待信号处理
            
            Console.WriteLine("关闭客户端和服务器");
            clientHandle.Stop();
            serverHandle.Stop();
            
            Task.Delay(1000).Wait(); // 等待信号处理
            Console.WriteLine("=== 测试完成 ===");
        }
    }
}