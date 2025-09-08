using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace XMSDK.Framework.Communication.SimpleTCP;

public class SocketServer
{
    private readonly string _host;
    private readonly int _port;
    private readonly Action<SocketServer, TcpClient> _onClientConnected;
    private readonly Action<SocketServer, TcpClient> _onClientDisconnected;
    private readonly Action<SocketServer, TcpClient, string> _onMessage;
    private readonly TimeSpan _heartbeatTimeout;
    private readonly TimeSpan _heartbeatInterval;
    private readonly int _maxMessageLength;
    private readonly int _maxClientCount;
    private readonly Dictionary<string, SignalHandler> _signals;
    private readonly Dictionary<string, CommandHandler> _commands;

    private TcpListener _listener;
    private volatile bool _isRunning;
    private Thread _listenerThread;

    private readonly ConcurrentDictionary<TcpClient, ClientInfo> _clients = new();

    private readonly object _signalLock = new();

    public IReadOnlyList<TcpClient> Clients => _clients.Keys.ToList();

    internal SocketServer(string host, int port, Action<SocketServer, TcpClient> onClientConnected,
        Action<SocketServer, TcpClient> onClientDisconnected, Action<SocketServer, TcpClient, string> onMessage,
        TimeSpan heartbeatTimeout, TimeSpan heartbeatInterval, int maxMessageLength, int maxClientCount,
        Dictionary<string, SignalHandler>? signals, Dictionary<string, CommandHandler>? commands)
    {
        _host = host;
        _port = port;
        _onClientConnected = onClientConnected;
        _onClientDisconnected = onClientDisconnected;
        _onMessage = onMessage;
        _heartbeatTimeout = heartbeatTimeout;
        _heartbeatInterval = heartbeatInterval;
        _maxMessageLength = maxMessageLength;
        _maxClientCount = maxClientCount;
        _signals = signals ?? new Dictionary<string, SignalHandler>();
        _commands = commands ?? new Dictionary<string, CommandHandler>();
    }

    /// <summary>
    /// 启动服务器
    /// </summary>
    public void Run()
    {
        if (_isRunning)
            return;

        _isRunning = true;
        _listener = new TcpListener(IPAddress.Parse(_host), _port);
        _listener.Start();

        _listenerThread = new Thread(ListenForClients)
        {
            IsBackground = true,
            Name = "SocketServer-Listener"
        };
        _listenerThread.Start();

        // 启动心跳检查线程
        var heartbeatThread = new Thread(HeartbeatChecker)
        {
            IsBackground = true,
            Name = "SocketServer-Heartbeat"
        };
        heartbeatThread.Start();
    }

    /// <summary>
    /// 关闭服务器
    /// </summary>
    public void Stop()
    {
        _isRunning = false;
        _listener?.Stop();

        // 关闭所有客户端连接
        foreach (var client in _clients.Keys.ToList())
        {
            Close(client);
        }
    }

    /// <summary>
    /// 向所有客户端广播消息
    /// </summary>
    /// <param name="message"></param>
    public void Broadcast(string message)
    {
        var clients = _clients.Keys.ToList();
        foreach (var client in clients)
        {
            SendToClient(client, message);
        }
    }

    /// <summary>
    /// 向除指定客户端外的所有客户端广播消息
    /// </summary>
    /// <param name="excludeClient"></param>
    /// <param name="message"></param>
    public void BroadcastExclude(TcpClient excludeClient, string message)
    {
        var clients = _clients.Keys.Where(c => c != excludeClient).ToList();
        foreach (var client in clients)
        {
            SendToClient(client, message);
        }
    }

    /// <summary>
    /// 设定信号的值并广播给所有客户端
    /// </summary>
    /// <param name="name"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    public void Signal<T>(string name, T value)
    {
        lock (_signalLock)
        {
            if (!_signals.TryGetValue(name, out var handler)) return;

            var oldValue = handler.GetValue();
                
            // 只有在值真正发生变化时才处理
            if (Equals(oldValue, value)) return;
                
            handler.SetValue(value);

            // 通知所有客户端信号变化
            var message = MessageProtocol.FormatSignalMessage(name, value);
            Broadcast(message);

            // 触发信号变化回调
            handler.InvokeChanged(this, null, oldValue, value);
        }
    }

    /// <summary>
    /// 执行命令并广播给所有客户端
    /// </summary>
    /// <param name="name"></param>
    public void Command(string name)
    {
        if (_commands.TryGetValue(name, out var handler))
        {
            var message = MessageProtocol.FormatCommandMessage(name);
            Broadcast(message);
            handler.InvokeServer(this, null);
        }
    }

    /// <summary>
    /// 关闭并移除一个客户端连接
    /// </summary>
    /// <param name="client"></param>
    public void Close(TcpClient client)
    {
        if (!_clients.TryRemove(client, out var clientInfo)) return;
        try
        {
            _onClientDisconnected?.Invoke(this, client);
            clientInfo.CancellationTokenSource.Cancel();
            client.Close();
        }
        catch (Exception ex)
        {
            // 记录异常但不抛出
            Console.WriteLine($"Error closing client: {ex.Message}");
        }
    }

    private void ListenForClients()
    {
        while (_isRunning)
        {
            try
            {
                var client = _listener.AcceptTcpClient();

                if (_clients.Count >= _maxClientCount)
                {
                    client.Close();
                    continue;
                }

                var clientInfo = new ClientInfo
                {
                    Client = client,
                    LastHeartbeat = DateTime.UtcNow,
                    CancellationTokenSource = new CancellationTokenSource()
                };

                _clients[client] = clientInfo;

                // 为每个客户端启动处理线程
                var clientThread = new Thread(() => HandleClient(client, clientInfo.CancellationTokenSource.Token))
                {
                    IsBackground = true,
                    Name = $"SocketServer-Client-{client.GetHashCode()}"
                };
                clientThread.Start();

                // 同步所有信号给新客户端
                SyncSignalsToClient(client);

                _onClientConnected?.Invoke(this, client);
            }
            catch (ObjectDisposedException)
            {
                // 服务器正在关闭
                break;
            }
            catch (Exception ex)
            {
                if (_isRunning) // 仅在服务器仍在运行时记录错误
                    Console.WriteLine($"Error accepting client: {ex.Message}");
            }
        }
    }

    private void HandleClient(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[_maxMessageLength];

            while (!cancellationToken.IsCancellationRequested && client.Connected)
            {
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0)
                    break;

                var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                var lines = message.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var line in lines)
                {
                    ProcessMessage(client, line.Trim());
                }

                // 更新心跳时间
                if (_clients.TryGetValue(client, out var clientInfo))
                {
                    clientInfo.LastHeartbeat = DateTime.UtcNow;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling client: {ex.Message}");
        }
        finally
        {
            Close(client);
        }
    }

    private void ProcessMessage(TcpClient client, string message)
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
                        
                    // 只有在值真正发生变化时才处理
                    if (!Equals(oldValue, newValue))
                    {
                        handler.SetValue(newValue);

                        // 广播给其他客户端
                        BroadcastExclude(client, message);

                        // 触发信号变化回调
                        handler.InvokeChanged(this, client, oldValue, newValue);
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
                // 广播给其他客户端
                BroadcastExclude(client, message);

                // 执行命令
                handler.InvokeServer(this, client);
            }

            return;
        }

        // 处理普通消息
        _onMessage?.Invoke(this, client, MessageProtocol.UnescapeMultiLine(message));
    }

    private void SyncSignalsToClient(TcpClient client)
    {
        lock (_signalLock)
        {
            foreach (var kvp in _signals)
            {
                var message = MessageProtocol.FormatSignalMessage(kvp.Key, kvp.Value.GetValue());
                SendToClient(client, message);
            }
        }
    }

    private void SendToClient(TcpClient? client, string message)
    {
        try
        {
            if (client?.Connected == true)
            {
                var data = Encoding.UTF8.GetBytes(MessageProtocol.EscapeMultiLine(message) + "\n");
                client.GetStream().Write(data, 0, data.Length);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending to client: {ex.Message}");
            Close(client);
        }
    }

    private void HeartbeatChecker()
    {
        while (_isRunning)
        {
            try
            {
                var now = DateTime.UtcNow;
                var clientsToRemove =
                    (from kvp in _clients
                        let client = kvp.Key
                        let clientInfo = kvp.Value
                        where now - clientInfo.LastHeartbeat > _heartbeatTimeout
                        select client).ToList();

                foreach (var client in clientsToRemove)
                {
                    Close(client);
                }

                Thread.Sleep(_heartbeatInterval);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in heartbeat checker: {ex.Message}");
            }
        }
    }

    private class ClientInfo
    {
        public TcpClient Client { get; set; }
        public DateTime LastHeartbeat { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}