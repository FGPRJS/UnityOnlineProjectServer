﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UnityOnlineProjectServer.Connection
{
    internal class Server
    {
        internal bool isRun;
        internal Socket socket;

        private long clientCount = 1;
        private ConcurrentDictionary<long, ConnectedClient> clients;

        private int pendingConnectionQueueCount = 100;
        internal int Port = 8080;

        internal Server()
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
                client.ShutdownRequestEvent += (id) => { ShutDownClient(id); };
                clients.TryAdd(i, client);
            }
        }

        internal void Start()
        {
            Console.WriteLine("Server Start");

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddress, Port);

            OpenServer(endPoint);
        }

        internal void StartLocal()
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
                isRun = true;
                InitializeHeartBeatTask();
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
                if(clients[i].ClientSocket == null)
                {
                    clients[i].Initialize(handler);
                }
            }
            

            Console.WriteLine("Client Connected");
        }

        #region HeartBeat

        private CancellationTokenSource heartbeatCancellationTokenSource;
        private CancellationToken heartbeatCancellationToken;
        private Task heartbeatTask;
        private void InitializeHeartBeatTask()
        {
            heartbeatCancellationTokenSource = new CancellationTokenSource();
            heartbeatCancellationToken = heartbeatCancellationTokenSource.Token;

            heartbeatTask = new Task(new Action(async () =>
            {
                while (isRun)
                {
                    await Task.Delay(Heartbeat.heartbeatInterval);
                    CheckHeartBeat();
                }
            }), heartbeatCancellationToken);
            heartbeatTask.Start();
        }
        private void CancelHeartBeatTask()
        {
            try
            {
                //Cancel heartbeat. if already cancellationrequested, cancel
                heartbeatCancellationToken.ThrowIfCancellationRequested();

                heartbeatCancellationTokenSource.Cancel();
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cancel HeartBeatTask is already requested.");
            }
        }
        private void CheckHeartBeat()
        {
            for(long i = 0; i < clientCount; i++)
            {
                var client = clients[i];
                if (client.ClientSocket == null) continue;
                if (client.heartbeat == null) continue;

                client.heartbeat.CountHeartbeat(Heartbeat.heartbeatInterval);
            }
        }

        #endregion

        #region ShutDown
        internal void ShutDownClient(long id)
        {
            var client = clients[id];

            if (client.ClientSocket != null)
            {
                client.ClientSocket.Shutdown(SocketShutdown.Both);
                client.ClientSocket.Close();
                client.ClientSocket = null;
            }
        }

        internal void ShutDownAllClients()
        {
            Console.WriteLine("ShutDown clients...");

            //All ShutDown
            for (long i = 0; i < clients.Count; i++)
            {
                ShutDownClient(clients[i].id);
            }

            Console.WriteLine("ShutDown clients complete!");
        }

        internal void ShutDownServer()
        {
            Console.WriteLine("Shutdown server...");

            ShutDownAllClients();

            isRun = false;
            CancelHeartBeatTask();

            //Server ShutDown
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            Console.WriteLine("Shutdown server complete!");
        }
        #endregion
    }
}
