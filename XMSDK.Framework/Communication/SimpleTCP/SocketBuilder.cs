namespace XMSDK.Framework.Communication.SimpleTCP;

/// <summary>
/// 创建 Socket 服务器和客户端的构建器，
/// 注意：此方法创建的 Socket 仅能保证回调外的通信过程中消息处理能够顺序执行，如果在回调中发生通信，仅能保证回调被顺序执行了，不能保证回调内的通信还能在回调处理时顺序执行。
/// </summary>
public static class SocketBuilder
{
    /// <summary>
    /// 创建一个 Socket 服务器构建器
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public static SocketServerBuilder Server(string host, int port)
    {
        return new SocketServerBuilder(host, port);
    }

    /// <summary>
    /// 创建一个 Socket 客户端构建器
    /// </summary>
    /// <param name="host"></param>
    /// <param name="port"></param>
    /// <returns></returns>
    public static SocketClientBuilder Client(string host, int port)
    {
        return new SocketClientBuilder(host, port);
    }
}