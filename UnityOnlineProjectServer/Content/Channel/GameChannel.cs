using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Connection;
using UnityOnlineProjectServer.Content.TickTasking;
using UnityOnlineProjectServer.Protocol;

namespace UnityOnlineProjectServer.Content.Map
{
    public class GameChannel
    {
        private static long clientCount = 16;
        private ConcurrentDictionary<long, ConnectedClient> clients;

        public BroadcastLocation broadcastLocation;

        public enum ChannelStatus
        {
            Disable,
            Enable,
            Full
        }
        public ChannelStatus status;

        public GameChannel()
        {
            BootChannel();
        }

        public void BootChannel()
        {
            //Initialize Broadcast
            broadcastLocation = new BroadcastLocation();
            broadcastLocation.TickEvent += BroadcastLocationTickEventAction;

            InitializeGlobalServerTask();

            status = ChannelStatus.Enable;

            //Create Clients
            clients = new ConcurrentDictionary<long, ConnectedClient>();
        }


        public bool AddClient(ConnectedClient client)
        {
            for(long i = 0; i < clientCount; i++)
            {
                if (!clients.ContainsKey(i))
                {
                    client.id = i;
                    client.ShutdownRequestEvent += RemoveClient;
                    client.PlayerObjectAssignedEvent += PlayerObjectAssigned;

                    clients.TryAdd(i, client);

                    //Setting ClientStatus
                    if (clients.Count >= clientCount)
                    {
                        status = ChannelStatus.Full;
                    }
                    else
                    {
                        status = ChannelStatus.Enable;
                    }

                    return true;
                }
            }

            //Cannot Access(Full)
            status = ChannelStatus.Full;
            return false;
        }

        

        public void RemoveClient(object sender, long id)
        {
            ConnectedClient client;
            clients.TryRemove(id, out client);

            BroadcastRemoveClient(client);

            client?.socket?.Close();
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
                CancelChannelTask();
            }
            else
            {
                _globalServerTaskCancellationTokenSource = new CancellationTokenSource();
                _globalServerTaskCancellationToken = _globalServerTaskCancellationTokenSource.Token;
            }

            _globalServerTask = new Task(new Action(async () =>
            {
                while (status != ChannelStatus.Disable)
                {
                    await Task.Delay(_tickInterval);

                    foreach (var client in clients.Values)
                    {
                        //CheckHeartbeat
                        client.heartbeat?.CountTick(_tickInterval);
                    }

                    //Send datas for all channel clients
                    broadcastLocation.CountTick(_tickInterval);
                }
            }), _globalServerTaskCancellationToken);

            _globalServerTask.Start();
        }

        private void CancelChannelTask()
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

        private void BroadcastLocationTickEventAction(object sender, EventArgs e)
        {
            foreach(var client in clients.Values)
            {
                BroadcastExistClient(client);
            }
        }

        private void PlayerObjectAssigned(object sender, EventArgs e)
        {
            BroadcastNewClient((ConnectedClient)sender);
        }

        private void BroadcastExistClient(ConnectedClient client)
        {
            foreach (var target in clients.Values)
            {
                if (target == client) continue;

                if (client.PlayerObject != null)
                {
                    var message = client.PlayerObject.CreateCurrentStatusMessage(MessageType.TankPositionReport);
                    target.SendTextData(message);
                }
                
            }
        }

        private void BroadcastNewClient(ConnectedClient client)
        {
            foreach (var target in clients.Values)
            {
                if ((client != null) && (!client.Equals(target))
                    && (client.PlayerObject != null))
                {
                    //client -> target
                    var message = client.PlayerObject.CreateObjectInfoMessage(MessageType.GameObjectSpawnReport);
                    target.SendTextData(message);

                    //target -> client
                    message = target.PlayerObject.CreateObjectInfoMessage(MessageType.GameObjectSpawnReport);
                    client.SendTextData(message);
                }
            }
        }

        private void BroadcastRemoveClient(ConnectedClient client)
        {
            foreach (var target in clients.Values)
            {
                if ((client != null) && (!client.Equals(target))
                    && (client.PlayerObject != null))
                {
                    var message = client.PlayerObject?.CreateCurrentStatusMessage(MessageType.GameObjectDestroyReport);
                    target.SendTextData(message);
                }
            }
        }

        #endregion // Global Tick Task


        #endregion // TickTask

        #region Shutdown

        public void ShutDownChannel()
        {
            status = ChannelStatus.Disable;

            broadcastLocation.TickEvent -= BroadcastLocationTickEventAction;

            CancelChannelTask();

            for (long id = 0; id < clientCount; id++)
            {
                RemoveClient(clients[id], id);
            }
        }

        #endregion

    }
}
