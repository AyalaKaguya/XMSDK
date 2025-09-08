using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace XMSDK.Framework.Communication.SimpleTCP;

public class SocketClient
{
    private readonly string _host;
    private readonly int _port;
    private readonly Action<SocketClient, string>? _onMessage;
    private readonly Dictionary<string, SignalHandler> _signals;
    private readonly Dictionary<string, CommandHandler> _commands;

    private TcpClient? _client;
    private NetworkStream? _stream;
    private volatile bool _isRunning;
    private Thread? _receiverThread;
    private readonly object _signalLock = new();

    public bool IsConnected => _client?.Connected == true;

    internal SocketClient(string host, int port, Action<SocketClient, string>? onMessage,
        Dictionary<string, SignalHandler>? signals, Dictionary<string, CommandHandler>? commands)
    {
        _host = host;
        _port = port;
        _onMessage = onMessage;
        _signals = signals ?? new Dictionary<string, SignalHandler>();
        _commands = commands ?? new Dictionary<string, CommandHandler>();
    }

    /// <summary>
    /// 启动客户端并连接到服务器
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Run()
    {
        if (_isRunning)
            return;

        try
        {
            _client = new TcpClient();
            _client.Connect(_host, _port);
            _stream = _client.GetStream();
            _isRunning = true;

            _receiverThread = new Thread(ReceiveMessages)
            {
                IsBackground = true,
                Name = "SocketClient-Receiver"
            };
            _receiverThread.Start();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to connect to server: {ex.Message}", ex);
        }
    }
        
    /// <summary>
    /// 关闭客户端连接
    /// </summary>
    public void Stop()
    {
        _isRunning = false;

        try
        {
            _stream?.Close();
            _client?.Close();
            _receiverThread?.Join(5000);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping client: {ex.Message}");
        }
    }

    /// <summary>
    /// 向服务器发送消息，消息不会被广播，除非服务器的OnMessage回调中有相应的处理
    /// </summary>
    /// <param name="message"></param>
    /// <exception cref="InvalidOperationException"></exception>
    public void Send(string message)
    {
        if (!IsConnected)
            throw new InvalidOperationException("Client is not connected");

        try
        {
            var data = Encoding.UTF8.GetBytes(MessageProtocol.EscapeMultiLine(message) + "\n");
            _stream?.Write(data, 0, data.Length);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to send message: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// 定义一个信号量，并发送信号到服务器
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    public void Signal<T>(string name, T value)
    {
        lock (_signalLock)
        {
            if (_signals.TryGetValue(name, out var handler))
            {
                var oldValue = handler.GetValue();
                    
                // 比较新旧值，只有在值真正发生变化时才处理
                if (!Equals(oldValue, value))
                {
                    handler.SetValue(value!);

                    // 发送信号到服务器
                    var message = MessageProtocol.FormatSignalMessage(name, value);
                    Send(message);

                    // 触发信号变化回调
                    handler.InvokeChangedClient(this, oldValue, value!);
                }
            }
        }
    }

    /// <summary>
    /// 定义一个命令
    /// </summary>
    /// <param name="name"></param>
    public void Command(string name)
    {
        if (_commands.TryGetValue(name, out var handler))
        {
            var message = MessageProtocol.FormatCommandMessage(name);
            Send(message);
            handler.InvokeClient(this);
        }
    }

    /// <summary>
    /// 获取信号的当前值
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public bool GetSignal<T>(string name, out T? value)
    {
        value = default;

        lock (_signalLock)
        {
            if (!_signals.TryGetValue(name, out var handler) || !(handler is SignalHandler<T>)) return false;
            value = (T)handler.GetValue();
            return true;
        }
    }

    private void ReceiveMessages()
    {
        var buffer = new byte[1024 * 1024]; // 1MB buffer

        while (_isRunning && IsConnected)
        {
            try
            {
                var bytesRead = _stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var lines = message.Split(['\n'], StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    ProcessMessage(line.Trim());
                }
            }
            catch (Exception ex)
            {
                if (_isRunning)
                {
                    Console.WriteLine($"Error receiving message: {ex.Message}");
                    break;
                }
            }
        }
    }

    private void ProcessMessage(string message)
    {
        if (string.IsNullOrEmpty(message))
            return;

        // 处理信号消息
        if (MessageProtocol.TryParseSignalMessage(message, out var signalName, out var signalValue))
        {
            lock (_signalLock)
            {
                if (_signals.TryGetValue(signalName, out var handler))
                {
                    var oldValue = handler.GetValue();
                    var newValue = MessageProtocol.ConvertValue(signalValue, handler.ValueType);
                        
                    // 只有在值真正发生变化时才触发回调
                    if (!Equals(oldValue, newValue))
                    {
                        handler.SetValue(newValue);
                        // 触发信号变化回调
                        handler.InvokeChangedClient(this, oldValue, newValue);
                    }
                }
            }
            return;
        }

        // 处理命令消息
        if (MessageProtocol.TryParseCommandMessage(message, out var commandName))
        {
            if (_commands.TryGetValue(commandName, out var handler))
            {
                handler.InvokeClient(this);
            }

            return;
        }

        // 处理普通消息
        _onMessage?.Invoke(this, MessageProtocol.UnescapeMultiLine(message));
    }
}