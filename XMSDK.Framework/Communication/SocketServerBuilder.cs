using System;
using System.Collections.Generic;
using System.Net.Sockets;

namespace XMSDK.Framework.Communication
{
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
        private readonly Dictionary<string, SignalHandler> _signals = new Dictionary<string, SignalHandler>();
        private readonly Dictionary<string, CommandHandler> _commands = new Dictionary<string, CommandHandler>();

        internal SocketServerBuilder(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public SocketServerBuilder OnClientConnected(Action<SocketServer, TcpClient> handler)
        {
            _onClientConnected = handler;
            return this;
        }

        public SocketServerBuilder OnClientDisconnected(Action<SocketServer, TcpClient> handler)
        {
            _onClientDisconnected = handler;
            return this;
        }

        public SocketServerBuilder OnMessage(Action<SocketServer, TcpClient, string> handler)
        {
            _onMessage = handler;
            return this;
        }

        public SocketServerBuilder HeartbeatTimeout(TimeSpan timeout)
        {
            _heartbeatTimeout = timeout;
            return this;
        }

        public SocketServerBuilder HeartbeatInterval(TimeSpan interval)
        {
            _heartbeatInterval = interval;
            return this;
        }

        public SocketServerBuilder MaxMessageLength(int length)
        {
            _maxMessageLength = length;
            return this;
        }

        public SocketServerBuilder MaxClientCount(int count)
        {
            _maxClientCount = count;
            return this;
        }

        public SocketServerBuilder Signal<T>(string name, T defaultValue, Action<SocketServer, TcpClient, T, T> onSignalChanged)
        {
            _signals[name] = new SignalHandler<T>(defaultValue, onSignalChanged);
            return this;
        }

        public SocketServerBuilder Command(string name, Action<SocketServer, TcpClient> onCommandExecuted)
        {
            _commands[name] = new CommandHandler(onCommandExecuted);
            return this;
        }

        public SocketServer Build()
        {
            return new SocketServer(_host, _port, _onClientConnected, _onClientDisconnected, _onMessage,
                _heartbeatTimeout, _heartbeatInterval, _maxMessageLength, _maxClientCount, _signals, _commands);
        }
    }
}
