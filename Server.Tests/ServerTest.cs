using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Connection;
using UnityOnlineProjectServer.Protocol;
using Xunit;
using static UnityOnlineProjectServer.Content.Pawn;

namespace Server.Tests
{
    public class ServerTest
    {
        GameServer server;
        ClientWebSocket client;

        private async Task StartandConnect()
        {
            server = new GameServer();
            server.StartLocal();

            await Connect();
        }

        private async Task Connect()
        {
            client = new ClientWebSocket();

            UriBuilder uriBuilder = new UriBuilder("ws", "127.0.0.1", 8080);
            await client.ConnectAsync(uriBuilder.Uri, CancellationToken.None);
        }

        [Fact]
        public async void ConnectAndDisconnect()
        {
            await StartandConnect();

            await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure,"", CancellationToken.None);
        }

        [Fact]
        public async void ConnectAndLoginRequest()
        {
            await StartandConnect();

            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult result = null;

            _ = client.SendAsync(CommunicationUtility.Serialize(new CommunicationMessage<Dictionary<string, string>>()
            {
                header = new Header()
                {
                    MessageName = MessageType.LoginRequest.ToString(),
                },
                body = new Body<Dictionary<string, string>>()
                {
                    Any = new Dictionary<string, string>()
                    {
                        ["UserName"] = "TESTER"
                    }
                }
            }), WebSocketMessageType.Text, true, CancellationToken.None);

            do
            {
                result = await client.ReceiveAsync(buffer, CancellationToken.None);
            }
            while (!result.EndOfMessage);
            
            var message = CommunicationUtility.Deserialize(buffer.Array);


            Assert.Equal("TESTER", message.body.Any["UserName"]);
        }

        [Fact]
        public async void SendPawnRequest()
        {
            await StartandConnect();

            ArraySegment<byte> buffer = new ArraySegment<byte>(new byte[1024]);
            WebSocketReceiveResult result = null;

            _ = client.SendAsync(CommunicationUtility.Serialize(new CommunicationMessage<Dictionary<string, string>>()
            {
                header = new Header()
                {
                    MessageName = MessageType.PlayerTankSpawnRequest.ToString(),
                },
                body = new Body<Dictionary<string, string>>()
                {
                    Any = new Dictionary<string, string>()
                    {
                        ["ObjectType"] = PawnType.Tank.ToString()
                    }
                }
            }), WebSocketMessageType.Text, true, CancellationToken.None);

            bool isRun = true;

            while (isRun)
            {
                do
                {
                    result = await client.ReceiveAsync(buffer, CancellationToken.None);
                }
                while (!result.EndOfMessage);

                var message = CommunicationUtility.Deserialize(buffer.Array);

                if (message != null)
                {
                    isRun = false;
                }
            }
        }
    }
}
