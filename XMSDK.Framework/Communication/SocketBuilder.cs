using System;
using System.Net;
using System.Net.Sockets;

namespace XMSDK.Framework.Communication
{
    public class SocketBuilder
    {
        public static SocketServerBuilder Server(string host, int port)
        {
            return new SocketServerBuilder(host, port);
        }

        public static SocketClientBuilder Client(string host, int port)
        {
            return new SocketClientBuilder(host, port);
        }

        private void Test()
        {
            // 我大概想这么实现：
            var serverHandle = SocketBuilder.Server("0.0.0.0", 8000) // 这里返回SocketServerBuilder的实例
                .OnClientConnected((server, client) =>
                {
                    // 处理客户端连接
                    Console.WriteLine($"Client connected: {client.Client.RemoteEndPoint}");
                })
                .OnClientDisconnected((server, client) =>
                {
                    // 处理客户端断开连接
                    Console.WriteLine($"Client disconnected: {client.Client.RemoteEndPoint}");
                })
                .HeartbeatTimeout(TimeSpan.FromSeconds(60)) // 设置心跳超时时间，如果超出这个时间没有收到客户端的心跳包，则认为客户端已经断开连接
                .HeartbeatInterval(TimeSpan.FromSeconds(30)) // 设置心跳间隔
                .MaxMessageLength(1024 * 1024) // 设置最大消息长度，
                .MaxClientCount(100) // 设置最大客户端连接数
                .OnMessage((server, client, message) =>
                {
                    // 处理客户端发送的消息
                    Console.WriteLine($"Message from {client.Client.RemoteEndPoint}: {message}");
                })
                .Signal<bool>("D2816", false, (server, client, oldValue, newValue) =>
                {
                    // 处理信号变化，服务器维护一份信号表，所有客户端都操作同一份信号表
                    Console.WriteLine($"Signal D2816 from {oldValue} changed to {newValue}");
                    server.Broadcast($"Signal D2816 changed to {newValue}"); // 广播信号变化
                    server.BroadcastExclude(client, $"Signal D2816 changed to {newValue}"); // 向其他客户端广播信号变化
                })
                .Command("OP124", (server, client) =>
                {
                    // 处理命令执行
                    Console.WriteLine($"Command OP124 executed by {client.Client.RemoteEndPoint}");
                })
                .Build(); // 构建SocketServer实例

            serverHandle.Run(); // 启动服务器，这会占用当前线程，所以实际使用中都需要额外创建一个线程
            serverHandle.Broadcast("Hello, clients!"); // 广播消息给所有连接的客户端
            serverHandle.Signal<bool>("D2816", true); // 设置信号D2816为true
            serverHandle.Command("OP124"); // 执行命令OP124
            serverHandle.Close(serverHandle.Clients[0]); // 关闭与服务器链接的某客户端
            serverHandle.Stop(); // 停止服务器，关闭所有客户端，拒绝新的连接
            // 服务器将会在单独的线程中运行，并为每一个客户端创建单独的线程，协议的处理异步执行。
            // 信息以文本的形式发送，将多行的信息转义为一行发送，接收时会自动分割成多行。
            // 信号以$sigName=value的形式发送，接收时会自动解析为信号名和值，如果value是字符串则需以双引号包裹，并转义多行文本到单行，同样在接收是转义回来。
            // 命令以#cmdName的形式发送，接收时会自动解析为命令名，执行时会调用onCommandExecuted回调。
            
            var clientHandle = SocketBuilder.Client("127.0.0.1", 8000) // 这里返回SocketClientBuilder的实例
                .OnMessage((client, message) =>
                {
                    // 处理服务器发送的消息
                    Console.WriteLine($"Message from server: {message}");
                })
                .Signal<bool>("D2816", false, (client, oldValue, newValue) =>
                {
                    // 监听信号变化，连接服务器的时候进行首次同步，与默认值不同时也会触发信号变换
                    Console.WriteLine($"Signal D2816 from {oldValue} changed to {newValue}");
                    client.Command("OP124"); // 执行命令
                })
                .Build(); // 构建SocketClient实例
            
            clientHandle.Run(); // 连接到服务器，这也会占用当前线程，所以实际使用中都需要额外创建一个线程
            clientHandle.Send("Hello, server!"); // 发送消息给服务器
            clientHandle.Signal<bool>("D2816", true); // 设置信号D2816为true
            clientHandle.Command("OP124"); // 执行命令
            clientHandle.GetSignal<bool>("D2816", out var value); // 获取信号D2816的值
            Console.WriteLine($"Signal D2816 value: {value}");
            clientHandle.Stop(); // 停止客户端，关闭与服务器的连接
        }
    }
}