using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace XMSDK.Framework.Communication.SimpleTCP;

public class SocketServerBuilder
{
    private readonly string _host;
    private readonly int _port;
    private Action<SocketServer, TcpClient> _onClientConnected;
    private Action<SocketServer, TcpClient> _onClientDisconnected;
    private Action<SocketServer, TcpClient, string> _onMessage;
    private TimeSpan _heartbeatTimeout = TimeSpan.FromSeconds(60);
    private TimeSpan _heartbeatInterval = TimeSpan.FromSeconds(30);
    private int _maxMessageLength = 1024 * 1024;
    private int _maxClientCount = 100;
    private readonly Dictionary<string, SignalHandler> _signals = new();
    private readonly Dictionary<string, CommandHandler> _commands = new();

    internal SocketServerBuilder(string host, int port)
    {
        _host = host;
        _port = port;
    }

    /// <summary>
    /// 当有客户端连接时触发
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    public SocketServerBuilder OnClientConnected(Action<SocketServer, TcpClient> handler)
    {
        _onClientConnected = handler;
        return this;
    }

    /// <summary>
    /// 当有客户端断开连接时触发
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    public SocketServerBuilder OnClientDisconnected(Action<SocketServer, TcpClient> handler)
    {
        _onClientDisconnected = handler;
        return this;
    }

    /// <summary>
    /// 当有客户端发送消息时触发
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    public SocketServerBuilder OnMessage(Action<SocketServer, TcpClient, string> handler)
    {
        _onMessage = handler;
        return this;
    }
        
    /// <summary>
    /// 设定心跳超时时间，超过此时间未收到客户端心跳则断开连接，默认60秒
    /// </summary>
    /// <param name="timeout"></param>
    /// <returns></returns>
    public SocketServerBuilder HeartbeatTimeout(TimeSpan timeout)
    {
        _heartbeatTimeout = timeout;
        return this;
    }

    /// <summary>
    /// 设定心跳间隔时间，默认30秒
    /// </summary>
    /// <param name="interval"></param>
    /// <returns></returns>
    public SocketServerBuilder HeartbeatInterval(TimeSpan interval)
    {
        _heartbeatInterval = interval;
        return this;
    }

    /// <summary>
    /// 设定单条消息的最大长度，默认1MB
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    public SocketServerBuilder MaxMessageLength(int length)
    {
        _maxMessageLength = length;
        return this;
    }

    /// <summary>
    /// 设定最大客户端连接数，默认100
    /// </summary>
    /// <param name="count"></param>
    /// <returns></returns>
    public SocketServerBuilder MaxClientCount(int count)
    {
        _maxClientCount = count;
        return this;
    }

    /// <summary>
    /// 创建一个信号量，信号量的值可以在客户端和服务器端互相修改，当值被修改时会触发回调
    /// </summary>
    /// <param name="name"></param>
    /// <param name="defaultValue"></param>
    /// <param name="onSignalChanged"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public SocketServerBuilder Signal<T>(string name, T defaultValue, Action<SocketServer, TcpClient, T, T> onSignalChanged)
    {
        _signals[name] = new SignalHandler<T>(defaultValue, onSignalChanged);
        return this;
    }

    /// <summary>
    /// 创建一个命令，客户端可以发送命令请求服务器执行，服务器会执行回调，
    /// 你可以理解为一个没有数值变化的信号量。
    /// </summary>
    /// <param name="name"></param>
    /// <param name="onCommandExecuted"></param>
    /// <returns></returns>
    public SocketServerBuilder Command(string name, Action<SocketServer, TcpClient> onCommandExecuted)
    {
        _commands[name] = new CommandHandler(onCommandExecuted);
        return this;
    }

    /// <summary>
    /// 实例化一个 Socket 服务器
    /// </summary>
    /// <returns></returns>
    public SocketServer Build()
    {
        return new SocketServer(_host, _port, _onClientConnected, _onClientDisconnected, _onMessage,
            _heartbeatTimeout, _heartbeatInterval, _maxMessageLength, _maxClientCount, _signals, _commands);
    }
}