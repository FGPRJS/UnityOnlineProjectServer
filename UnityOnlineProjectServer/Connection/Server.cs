using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace UnityOnlineProjectServer.Connection
{
    internal class Server
    {
        public Socket socket;

        private int PendingConnectionQueueCount = 100;
        public int Port = 8080;

        public Server()
        {
            // Create Socket
            socket = new Socket(
                AddressFamily.InterNetwork, 
                SocketType.Stream, 
                ProtocolType.Tcp);
        }

        public void Start()
        {
            Console.WriteLine("Server Start");

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, Port);

            OpenServer(endPoint);
        }

        public void StartLocal()
        {
            Console.WriteLine("Server Start");

            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, Port);

            OpenServer(endPoint);
        }

        private void OpenServer(IPEndPoint endPoint)
        {
            try
            {
                socket.Bind(endPoint);
                socket.Listen(PendingConnectionQueueCount);
                socket.BeginAccept(ClientAccepted, socket);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Open Failed. Reason : " + ex.Message);
            }
        }

        private void ClientAccepted(IAsyncResult ar)
        {
            // Get the socket that handles the client request.  
            var clientSocket = (Socket)ar.AsyncState;
            Socket handler = clientSocket.EndAccept(ar);

            Console.WriteLine("Client Connected");
        }

        public void SendData()
        {

        }

        public void ShutDown()
        {
            socket.Close();
        }
    }
}
