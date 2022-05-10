﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Connection.TickTasking;
using UnityOnlineProjectServer.Content;
using UnityOnlineProjectServer.Content.Map;

namespace UnityOnlineProjectServer.Connection
{
    public class GameServer
    {
        public bool isRun;
        public Socket socket;

        private readonly long clientCount = 100;
        private ConcurrentDictionary<long, ConnectedClient> clients;

        private readonly long gameFieldCount = 1;
        private ConcurrentDictionary<long, GameField> gameFields;

        private int pendingConnectionQueueCount = 100;
        public int Port = 8080;

        public GameServer()
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

            //Create Maps
            gameFields = new ConcurrentDictionary<long, GameField>();
            
            for(long i = 0; i < gameFieldCount; i++)
            {
                var map = new GameField();
                map.isEnterable = true;
                gameFields.TryAdd(i, map);
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
                isRun = true;
                InitializeGlobalServerTask();
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

            GameField availableField = null;

            //Find Map
            for (long mapIndex = 0; mapIndex < gameFields.Count; mapIndex++)
            {
                var map = gameFields[mapIndex];

                if (map.isEnterable)
                {
                    availableField = map;
                    break;
                }
            }

            //Find Client
            for (long i = 0; i < clients.Count; i++)
            {
                if(clients[i].ClientSocket == null)
                {
                    clients[i].Initialize(handler);
                    clients[i].currentField = availableField;
                    Console.WriteLine("Client Connected");
                    return;
                }
            }

            //Client Full. Cannot Connect
        }

        #region Tick Task

        #region Global Tick Task

        private CancellationTokenSource _globalServerTaskCancellationTokenSource;
        private CancellationToken _globalServerTaskCancellationToken;
        private Task _globalServerTask;
        private int _tickInterval = 100;

        private void InitializeGlobalServerTask()
        {
            if (_globalServerTaskCancellationTokenSource != null)
            {
                CancelGlobalServerTask();
            }
            else
            {
                _globalServerTaskCancellationTokenSource = new CancellationTokenSource();
                _globalServerTaskCancellationToken = _globalServerTaskCancellationTokenSource.Token;
            }

            _globalServerTask = new Task(new Action(async () =>
            {
                while (isRun)
                {
                    await Task.Delay(_tickInterval);
                    
                    for (long i = 0; i < clientCount; i++)
                    {
                        var client = clients[i];
                        //CheckHeartbeat
                        client?.heartbeat?.CountTick(_tickInterval);
                        //CheckPositionReport
                        client?.playerObject?.positionReport?.CountTick(_tickInterval);
                    }
                }
            }), _globalServerTaskCancellationToken);

            _globalServerTask.Start();
        }

        private void CancelGlobalServerTask()
        {
            try
            {
                //Cancel heartbeat. if already cancellationrequested, cancel
                _globalServerTaskCancellationToken.ThrowIfCancellationRequested();

                _globalServerTaskCancellationTokenSource.Cancel();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cancel GlobalTask is already requested.");
            }
        }

        #endregion // Global Tick Task

        
        #endregion // TickTask

        #region ShutDown
        public void ShutDownClient(long id)
        {
            var client = clients[id];

            if (client.ClientSocket != null)
            {
                client.ClientSocket.Shutdown(SocketShutdown.Both);
                client.ClientSocket.Close();
                client.ClientSocket = null;
            }
        }

        public void ShutDownAllClients()
        {
            Console.WriteLine("ShutDown clients...");

            //All ShutDown
            for (long i = 0; i < clients.Count; i++)
            {
                ShutDownClient(clients[i].id);
            }

            Console.WriteLine("ShutDown clients complete!");
        }

        public void ShutDownServer()
        {
            Console.WriteLine("Shutdown server...");

            ShutDownAllClients();

            isRun = false;
            CancelGlobalServerTask();

            //Server ShutDown
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();

            Console.WriteLine("Shutdown server complete!");
        }
        #endregion
    }
}