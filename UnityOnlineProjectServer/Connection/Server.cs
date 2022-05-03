using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityOnlineProjectProtocol;
using UnityOnlineProjectProtocol.Protocol;

namespace UnityOnlineProjectServer.Connection
{
    internal class Server
    {
        public Socket socket;

        private long clientCount = 100;
        private ConcurrentDictionary<long, ConnectedClient> clients;

        private int pendingConnectionQueueCount = 100;
        public int Port = 8080;

        public Server()
        {
            // Create Socket
            socket = new Socket(
                AddressFamily.InterNetwork, 
                SocketType.Stream, 
                ProtocolType.Tcp);

            //Create Clients
            clients = new ConcurrentDictionary<long, ConnectedClient>();

            for(long i = 0; i < clientCount; i++)
            {
                var client = new ConnectedClient(i);
                clients.TryAdd(i, client);
            }
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
                socket.Listen(pendingConnectionQueueCount);
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
            // Wait for other Client
            socket.BeginAccept(ClientAccepted, socket);

            for (long i = 0; i < clients.Count; i++)
            {
                if(clients[i].socket == null)
                {
                    clients[i].Initialize(handler);
                }
            }
            

            Console.WriteLine("Client Connected");
        }

        public void SendDataTo(Socket handler, CommunicationMessage message)
        {
            // Convert the string data to byte data using ASCII encoding.  
            var byteData = CommunicationUtility.Serialize(message);

            // Begin sending the data to the remote device.  
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), socket);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.  
                Socket handler = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.  
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);
            }
            catch (Exception e)
            {
                Console.WriteLine("Send Failed. Reason : " + e.Message);
            }
        }

        public void ShutDown()
        {
            Console.WriteLine("ShutDown Server...");

            //All ShutDown
            for (long i = 0; i < clients.Count; i++)
            {
                var client = clients[i];

                if (client.socket != null)
                {
                    client.socket.Shutdown(SocketShutdown.Both);
                    client.socket.Close();
                    client.socket = null;
                }
            }

            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            Console.WriteLine("ShutDown Complete!");
        }
    }
}
