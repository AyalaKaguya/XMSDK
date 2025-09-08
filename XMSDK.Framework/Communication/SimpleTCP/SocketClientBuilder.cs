using System;
using System.Collections.Generic;

namespace XMSDK.Framework.Communication.SimpleTCP;

public class SocketClientBuilder
{
    private readonly string _host;
    private readonly int _port;
    private Action<SocketClient, string>? _onMessage;
    private readonly Dictionary<string, SignalHandler> _signals = new();
    private readonly Dictionary<string, CommandHandler> _commands = new();

    internal SocketClientBuilder(string host, int port)
    {
        _host = host;
        _port = port;
    }

    /// <summary>
    /// 当有服务器发送消息时触发
    /// </summary>
    /// <param name="handler"></param>
    /// <returns></returns>
    public SocketClientBuilder OnMessage(Action<SocketClient, string> handler)
    {
        _onMessage = handler;
        return this;
    }

    /// <summary>
    /// 当信号值变化时触发
    /// </summary>
    /// <param name="name"></param>
    /// <param name="defaultValue"></param>
    /// <param name="onSignalChanged"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public SocketClientBuilder Signal<T>(string name, T defaultValue, Action<SocketClient, T, T> onSignalChanged)
    {
        _signals[name] = new SignalHandler<T>(defaultValue, onSignalChanged);
        return this;
    }

    /// <summary>
    /// 当命令被执行时触发
    /// </summary>
    /// <param name="name"></param>
    /// <param name="onCommandExecuted"></param>
    /// <returns></returns>
    public SocketClientBuilder Command(string name, Action<SocketClient> onCommandExecuted)
    {
        _commands[name] = new CommandHandler(onCommandExecuted);
        return this;
    }

    public SocketClient Build()
    {
        return new SocketClient(_host, _port, _onMessage, _signals, _commands);
    }
}