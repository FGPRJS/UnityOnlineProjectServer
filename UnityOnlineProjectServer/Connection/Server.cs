using System;
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

namespace UnityOnlineProjectServer.Connection
{
    public class Server
    {
        public FieldMap Map;
        public NearbyObjPositionReport posReport;

        public bool isRun;
        public Socket socket;

        private long clientCount = 100;
        private ConcurrentDictionary<long, ConnectedClient> clients;

        private int pendingConnectionQueueCount = 100;
        public int Port = 8080;

        public Server()
        {
            //Create Map
            Map = new FieldMap(1000, 1000, 10, 10, 1);

            //Create PositionReport
            posReport = new NearbyObjPositionReport();
            posReport.TickEvent += ReportObjectPosition;

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

            for (long i = 0; i < clients.Count; i++)
            {
                if(clients[i].ClientSocket == null)
                {
                    clients[i].Initialize(handler);
                }
            }
            

            Console.WriteLine("Client Connected");
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

                    //Tick Action
                    posReport.CountTick(_tickInterval);
                    
                    for (long i = 0; i < clientCount; i++)
                    {
                        var client = clients[i];
                        //CheckHeartbeat
                        client?.heartbeat?.CountTick(_tickInterval);
                        //CheckPositionReport
                        client?.playerObject?.positionReport?.CountTick(_tickInterval);
                        //
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

        private void ReportObjectPosition(object sender, EventArgs e)
        {
            for (long i = 0; i < clientCount; i++)
            {
                var client = clients[i];
                var clientObj = client?.playerObject;

                if (clientObj == null) continue;
                var region = Map.GetAppropriateRegion(clientObj);
                var nearbyObjs = region.GetAllNearbyGameObjects();
                foreach(var obj in nearbyObjs)
                {
                    var message = obj.CreateCurrentStatusMessage();
                    client.SendData(message);
                }
            }
        }

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
