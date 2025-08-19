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
    }
}