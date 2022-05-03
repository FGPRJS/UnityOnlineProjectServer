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
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            try
            {
                socket.Bind(localEndPoint);
                socket.Listen(PendingConnectionQueueCount);

                while (true)
                {
                    socket.BeginAccept(ClientAccepted, new AsyncStateObject());
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Open Failed. Reason : " + ex.Message);
            }
        }

        public void StartLocal()
        {
            Console.WriteLine("Server Open");

            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            try
            {
                socket.Bind(localEndPoint);
                socket.Listen(PendingConnectionQueueCount);

                while (true)
                {
                    socket.BeginAccept(ClientAccepted, new AsyncStateObject());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Open Failed. Reason : " + ex.Message);
            }
        }

        private void ClientAccepted(IAsyncResult ar)
        {
            // Get the socket that handles the client request.  
            Socket clientSocket = (Socket)ar.AsyncState;
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
