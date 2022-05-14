using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityOnlineProjectServer.Connection;
using Xunit;

namespace Server.Tests
{
    public class ServerTest
    {
        [Fact]
        public void ConnectAndDisconnect()
        {
            AutoResetEvent breaker = new AutoResetEvent(false);

            GameServer server = new GameServer();

            server.StartLocal();

            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            IPAddress address = IPAddress.Parse("127.0.0.1");
            IPEndPoint endPoint = new IPEndPoint(address, 8080);
            client.BeginConnect(endPoint, 
                (ar) => {
                    client.EndConnect(ar);
                    breaker.Set();
                }, client);

            breaker.WaitOne();

            client.Shutdown(SocketShutdown.Both);
            client.Close();
        }
    }
}
