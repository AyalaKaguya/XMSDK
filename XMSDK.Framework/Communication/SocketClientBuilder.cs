using System;
using System.Collections.Generic;

namespace XMSDK.Framework.Communication
{
    public class SocketClientBuilder
    {
        private readonly string _host;
        private readonly int _port;
        private Action<SocketClient, string> _onMessage;
        private readonly Dictionary<string, SignalHandler> _signals = new Dictionary<string, SignalHandler>();
        private readonly Dictionary<string, CommandHandler> _commands = new Dictionary<string, CommandHandler>();

        internal SocketClientBuilder(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public SocketClientBuilder OnMessage(Action<SocketClient, string> handler)
        {
            _onMessage = handler;
            return this;
        }

        public SocketClientBuilder Signal<T>(string name, T defaultValue, Action<SocketClient, T, T> onSignalChanged)
        {
            _signals[name] = new SignalHandler<T>(defaultValue, onSignalChanged);
            return this;
        }

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
}
