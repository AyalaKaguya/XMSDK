using System;
using System.Net.Sockets;

namespace XMSDK.Framework.Communication.SimpleTCP;

public abstract class SignalHandler
{
    public abstract Type ValueType { get; }
    public abstract object GetValue();
    public abstract void SetValue(object value);
    public abstract void InvokeChanged(SocketServer server, TcpClient? client, object oldValue, object newValue);
    public abstract void InvokeChangedClient(SocketClient client, object oldValue, object newValue);
}

public class SignalHandler<T> : SignalHandler
{
    private T _value;
    private readonly Action<SocketServer, TcpClient?, T, T>? _onServerSignalChanged;
    private readonly Action<SocketClient, T, T>? _onClientSignalChanged;

    public override Type ValueType => typeof(T);

    public SignalHandler(T defaultValue, Action<SocketServer, TcpClient?, T, T> onServerSignalChanged)
    {
        _value = defaultValue;
        _onServerSignalChanged = onServerSignalChanged;
    }

    public SignalHandler(T defaultValue, Action<SocketClient, T, T> onClientSignalChanged)
    {
        _value = defaultValue;
        _onClientSignalChanged = onClientSignalChanged;
    }

    public override object GetValue()
    {
        return _value ?? throw new NullReferenceException();
    }

    public override void SetValue(object value)
    {
        _value = (T)value;
    }

    public override void InvokeChanged(SocketServer server, TcpClient? client, object oldValue, object newValue)
    {
        _onServerSignalChanged?.Invoke(server, client, (T)oldValue, (T)newValue);
    }

    public override void InvokeChangedClient(SocketClient client, object oldValue, object newValue)
    {
        _onClientSignalChanged?.Invoke(client, (T)oldValue, (T)newValue);
    }
}