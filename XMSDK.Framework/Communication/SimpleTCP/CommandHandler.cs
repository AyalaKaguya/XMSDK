using System;
using System.Net.Sockets;

namespace XMSDK.Framework.Communication.SimpleTCP;

public class CommandHandler
{
    private readonly Action<SocketServer, TcpClient>? _onServerCommandExecuted;
    private readonly Action<SocketClient>? _onClientCommandExecuted;

    public CommandHandler(Action<SocketServer, TcpClient> onServerCommandExecuted)
    {
        _onServerCommandExecuted = onServerCommandExecuted;
    }

    public CommandHandler(Action<SocketClient> onClientCommandExecuted)
    {
        _onClientCommandExecuted = onClientCommandExecuted;
    }

    public void InvokeServer(SocketServer server, TcpClient client)
    {
        _onServerCommandExecuted?.Invoke(server, client);
    }

    public void InvokeClient(SocketClient client)
    {
        _onClientCommandExecuted?.Invoke(client);
    }
}