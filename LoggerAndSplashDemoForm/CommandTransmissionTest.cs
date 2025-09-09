using System;
using System.Threading.Tasks;
using XMSDK.Framework.Communication.SimpleTCP;

namespace LoggerAndSplashDemoForm;

public static class TransmissionTest
{
    public static void TestCommandTransmission()
    {
        Console.WriteLine("\n=== 测试命令透传功能 ===");

        // 创建服务器，注册命令处理器
        var server = SocketBuilder.Server("127.0.0.1", 9000)
            .Command("RESET_SYSTEM",
                (_, _) => { Console.WriteLine($"[服务器] 收到来自客户端的RESET_SYSTEM命令， 执行重置系统状态"); })
            .Command("UPDATE_CONFIG",
                (_, _) => { Console.WriteLine($"[服务器] 收到来自客户端的UPDATE_CONFIG命令， 执行更新配置文件"); })
            .OnClientConnected((_, client) =>
            {
                Console.WriteLine($"[服务器] 客户端已连接: {client.Client.RemoteEndPoint}");
            })
            .Build();

        server.Run();
        Task.Delay(500).Wait();

        // 创建第一个客户端
        var client1 = SocketBuilder.Client("127.0.0.1", 9000)
            .Command("RESET_SYSTEM", _ => { Console.WriteLine("[客户端1] 执行RESET_SYSTEM命令 - 重置系统状态"); })
            .Command("UPDATE_CONFIG", _ => { Console.WriteLine("[客户端1] 执行UPDATE_CONFIG命令 - 更新配置文件"); })
            .Build();

        // 创建第二个客户端
        var client2 = SocketBuilder.Client("127.0.0.1", 9000)
            .Command("RESET_SYSTEM", _ => { Console.WriteLine("[客户端2] 执行RESET_SYSTEM命令 - 清理缓存数据"); })
            .Command("UPDATE_CONFIG", _ => { Console.WriteLine("[客户端2] 执行UPDATE_CONFIG命令 - 重新加载配置"); })
            .Build();

        // 创建第三个客户端（不注册命令处理器）
        var client3 = SocketBuilder.Client("127.0.0.1", 9000)
            .OnMessage((_, message) => { Console.WriteLine($"[客户端3] 收到消息: {message}"); })
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

    public static void TestTransmissionCreate()
    {
        Console.WriteLine("\n=== 测试网络通信创建 ===");
        // 我大概想这么实现：
        var serverHandle = SocketBuilder.Server("0.0.0.0", 8000) // 这里返回SocketServerBuilder的实例
            .OnClientConnected((_, client) =>
            {
                // 处理客户端连接
                Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
            })
            .OnClientDisconnected((_, client) =>
            {
                // 处理客户端断开连接
                Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}");
            })
            .HeartbeatTimeout(TimeSpan.FromSeconds(60)) // 设置心跳超时时间，如果超出这个时间没有收到客户端的心跳包，则认为客户端已经断开连接
            .HeartbeatInterval(TimeSpan.FromSeconds(30)) // 设置心跳间隔
            .MaxMessageLength(1024 * 1024) // 设置最大消息长度，
            .MaxClientCount(100) // 设置最大客户端连接数
            .OnMessage((_, client, message) =>
            {
                // 处理客户端发送的消息
                Console.WriteLine($"Message from {client.Client.RemoteEndPoint}: {message}");
            })
            .Signal("D2816", true, (server, client, oldValue, newValue) =>
            {
                // 处理信号变化，服务器维护一份信号表，所有客户端都操作同一份信号表
                Console.WriteLine($"Server：Signal D2816 from {oldValue} changed to {newValue}");
                server.Broadcast($"Server：Signal D2816 changed to {newValue}"); // 广播信号变化
                server.BroadcastExclude(client, $"Server：Signal D2816 changed to {newValue}"); // 向其他客户端广播信号变化
            })
            .Command("OP124", (_, client) =>
            {
                Console.WriteLine(client == null
                    ? $"Command OP124 executed by Server"
                    : $"Command OP124 executed by {client.Client.RemoteEndPoint}");
            })
            .Build(); // 构建SocketServer实例

        serverHandle.Run(); // 启动服务器，这会占用当前线程，所以实际使用中都需要额外创建一个线程
        serverHandle.Broadcast("Hello, clients!"); // 广播消息给所有连接的客户端
        serverHandle.Signal("D2816", false); // 设置信号D2816为true
        serverHandle.Command("OP124"); // 执行命令OP124
        // 服务器将会在单独的线程中运行，并为每一个客户端创建单独的线程，协议的处理异步执行。
        // 信息以文本的形式发送，将多行的信息转义为一行发送，接收时会自动分割成多行。
        // 信号以$sigName=value的形式发送，接收时会自动解析为信号名和值，如果value是字符串则需以双引号包裹，并转义多行文本到单行，同样在接收是转义回来。
        // 命令以#cmdName的形式发送，接收时会自动解析为命令名，执行时会调用onCommandExecuted回调。

        var clientHandle = SocketBuilder.Client("127.0.0.1", 8000) // 这里返回SocketClientBuilder的实例
            .OnMessage((_, message) =>
            {
                // 处理服务器发送的消息
                Console.WriteLine($"Message from server: {message}");
            })
            .Signal("D2816", false, (client, oldValue, newValue) =>
            {
                // 监听信号变化
                // 如果新值与服务器一致，服务器不会触发回调
                Console.WriteLine($"Client：Signal D2816 from {oldValue} changed to {newValue}");
                client.Command("OP125"); // 执行命令
            })
            .Command("OP124", _ =>
            {
                // 处理命令执行, 如果任意客户端或者服务器发布了命令OP124，则会触发这个回调
                // 服务端的透传是先服务端的实现进行的
                Console.WriteLine($"Command OP124 executed on client");
            })
            .Command("OP125", _ =>
            {
                // 处理命令执行, 如果任意客户端或者服务器发布了命令OP124，则会触发这个回调
                // 服务端的透传是先服务端的实现进行的
                Console.WriteLine($"Command OP125 executed on client");
            })
            .Build(); // 构建SocketClient实例

        clientHandle.Run(); // 连接到服务器，这也会占用当前线程，所以实际使用中都需要额外创建一个线程
        Task.Delay(1000).Wait();
        clientHandle.Send("Hello, server!"); // 发送消息给服务器
        clientHandle.Signal("D2816", true); // 设置信号D2816为true
        clientHandle.Signal("D2816", false); // 设置信号D2816为true
        clientHandle.Command("OP124"); // 执行命令
        clientHandle.GetSignal<bool>("D2816", out var value); // 获取信号D2816的值
        Console.WriteLine($"Signal D2816 value: {value}");
        clientHandle.Stop(); // 停止客户端，关闭与服务器的连接


        // serverHandle.Close(serverHandle.Clients[0]); // 关闭与服务器链接的某客户端，有的时候上面停止的时候已经关闭了0号客户端，此时再关闭就会超出索引
        serverHandle.Stop(); // 停止服务器，关闭所有客户端，拒绝新的连接
        Console.WriteLine("=== 网络通信创建测试完成 ===\n");
    }

    public static void TestSignalTransmission()
    {
        // 测试网络通信
        Console.WriteLine("\n=== 测试网络通信信号量 ===");
        var serverHandle = SocketBuilder.Server("0.0.0.0", 8000)
            .Signal("D2816", false, (server, _, oldValue, newValue) =>
            {
                Console.WriteLine($"Signal D2816 changed from {oldValue} to {newValue}");
                server.Broadcast($"Client Message: Signal D2816 changed to {newValue}");
            })
            .OnClientConnected((_, client) =>
            {
                Console.WriteLine($"Client {client.Client.RemoteEndPoint} Connected");
            })
            .Build();

        serverHandle.Run();

        Task.Delay(1000).Wait(); // 等待服务器启动

        var clientHandle = SocketBuilder.Client("127.0.0.1", 8000)
            .Signal("D2816", false,
                (_, oldValue, newValue) =>
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
        Console.WriteLine("=== 通信信号测试完成 ===\n");
    }
}