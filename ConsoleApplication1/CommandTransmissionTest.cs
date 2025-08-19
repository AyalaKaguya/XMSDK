using System;
using System.Threading.Tasks;
using XMSDK.Framework.Communication;

namespace ConsoleApplication1
{
    public class CommandTransmissionTest
    {
        public static void TestCommandTransmission()
        {
            Console.WriteLine("\n=== 测试命令透传功能 ===");
            
            // 创建服务器，注册命令处理器
            var server = SocketBuilder.Server("127.0.0.1", 9000)
                .Command("RESET_SYSTEM", (srv, client) =>
                {
                    Console.WriteLine($"[服务器] 收到来自客户端的RESET_SYSTEM命令， 执行重置系统状态");
                })
                .Command("UPDATE_CONFIG", (srv, client) =>
                {
                    Console.WriteLine($"[服务器] 收到来自客户端的UPDATE_CONFIG命令， 执行更新配置文件");
                })
                .OnClientConnected((srv, client) =>
                {
                    Console.WriteLine($"[服务器] 客户端已连接: {client.Client.RemoteEndPoint}");
                })
                .Build();

            server.Run();
            Task.Delay(500).Wait();

            // 创建第一个客户端
            var client1 = SocketBuilder.Client("127.0.0.1", 9000)
                .Command("RESET_SYSTEM", (client) =>
                {
                    Console.WriteLine("[客户端1] 执行RESET_SYSTEM命令 - 重置系统状态");
                })
                .Command("UPDATE_CONFIG", (client) =>
                {
                    Console.WriteLine("[客户端1] 执行UPDATE_CONFIG命令 - 更新配置文件");
                })
                .Build();

            // 创建第二个客户端
            var client2 = SocketBuilder.Client("127.0.0.1", 9000)
                .Command("RESET_SYSTEM", (client) =>
                {
                    Console.WriteLine("[客户端2] 执行RESET_SYSTEM命令 - 清理缓存数据");
                })
                .Command("UPDATE_CONFIG", (client) =>
                {
                    Console.WriteLine("[客户端2] 执行UPDATE_CONFIG命令 - 重新加载配置");
                })
                .Build();

            // 创建第三个客户端（不注册命令处理器）
            var client3 = SocketBuilder.Client("127.0.0.1", 9000)
                .OnMessage((client, message) =>
                {
                    Console.WriteLine($"[客户端3] 收到消息: {message}");
                })
                .Build();

            // 连接所有客户端
            client1.Run();
            Task.Delay(300).Wait();
            
            client2.Run();
            Task.Delay(300).Wait();
            
            client3.Run();
            Task.Delay(300).Wait();

            Console.WriteLine("\n--- 客户端1发送RESET_SYSTEM命令 ---");
            client1.Command("RESET_SYSTEM");
            Task.Delay(500).Wait();

            Console.WriteLine("\n--- 客户端2发送UPDATE_CONFIG命令 ---");
            client2.Command("UPDATE_CONFIG");
            Task.Delay(500).Wait();

            Console.WriteLine("\n--- 服务器主动发送命令给所有客户端 ---");
            server.Command("RESET_SYSTEM");
            Task.Delay(500).Wait();

            // 清理资源
            Console.WriteLine("\n--- 关闭连接 ---");
            client1.Stop();
            client2.Stop();
            client3.Stop();
            server.Stop();
            
            Task.Delay(500).Wait();
            Console.WriteLine("=== 命令透传测试完成 ===\n");
        }
    }
}
