using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Content.Map;
using UnityOnlineProjectServer.Utility;

namespace UnityOnlineProjectServer.Connection
{
    public class GameServer
    {
        public bool isRun;
        public TcpListener listener;

        public int Port = 8080;

        public ConcurrentDictionary<ConnectedClient, bool> lobby;

        public static long ChannelCount = 1;
        public ConcurrentDictionary<long, GameChannel> channels;

        public GameServer()
        {
            lobby = new ConcurrentDictionary<ConnectedClient, bool>();
            channels = new ConcurrentDictionary<long, GameChannel>();

            for(long i = 0; i < ChannelCount; i++)
            {
                var newChannel = new GameChannel();

                channels.TryAdd(i, newChannel);
            }
        }

        public void Start()
        {
            Logger.Instance.InfoLog("Server Start");

            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            IPAddress ipAddress = ipHostInfo.AddressList[0];

            Logger.Instance.InfoLog("Server Start at " + ipAddress.ToString());

            listener = new TcpListener(ipAddress, Port);

            OpenServer();
        }

        public void StartLocal()
        {
            Logger.Instance.InfoLog("Server Start");

            IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
            IPAddress ipAddress = ipHostInfo.AddressList[1];
            listener = new TcpListener(ipAddress, Port);

            OpenServer();
        }

        private void OpenServer()
        {
            try
            {
                listener.Start();

                listener.BeginAcceptTcpClient(ClientAccepted, listener);
                isRun = true;
            }
            catch (Exception ex)
            {
                Logger.Instance.InfoLog("Open Failed. Reason : " + ex.Message);
            }
        }

        private void ClientAccepted(IAsyncResult ar)
        {
            Logger.Instance.InfoLog("Connecting client detected");

            // Get the socket that handles the client request.  
            var clientSocket = listener.EndAcceptTcpClient(ar);

            // Wait for other Client
            listener.BeginAcceptTcpClient(ClientAccepted, listener);

            //Create Client
            var client = new ConnectedClient();
            client.Initialize(clientSocket);
            client.HandshakeCompleteEvent += FindChannelForClient;

            //Keep client in lobby
            lobby.TryAdd(client, true);
        }

        private void FindChannelForClient(object sender, EventArgs e)
        {
            var client = (ConnectedClient)sender;

            client.HandshakeCompleteEvent -= FindChannelForClient;

            lobby.TryRemove(client, out var dummy);
            foreach(var channel in channels.Values)
            {
                var result = channel.AddClient(client);
                if (!result)
                {
                    //Cannot enter to channel;
                }
            }
        }

        #region ShutDown

        public void ShutdownAllChannels()
        {
            Logger.Instance.InfoLog("ShutDown channels...");

            //All ShutDown
            foreach(var channel in channels.Values)
            {
                channel.ShutDownChannel();
            }

            Logger.Instance.InfoLog("ShutDown channels complete!");
        }

        public void ShutDownServer()
        {
            Logger.Instance.InfoLog("Shutdown server...");

            ShutdownAllChannels();

            isRun = false;

            //Server ShutDown
            listener.Stop();

            Logger.Instance.InfoLog("Shutdown server complete!");
        }
        #endregion
    }
}
