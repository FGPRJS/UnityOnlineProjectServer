using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using UnityOnlineProjectProtocol;
using UnityOnlineProjectProtocol.Connection;

namespace UnityOnlineProjectServer.Connection
{
    internal class ConnectedClient
    {
        public Socket socket;

        public readonly long id;
        private byte[] receiveBuffer;

        public ConnectedClient(long id)
        {
            this.id = id;
        }

        public void Initialize(Socket socket)
        {
            this.socket = socket;
            socket.ReceiveBufferSize = AsyncStateObject.BufferSize;
            socket.SendBufferSize = AsyncStateObject.BufferSize;

            receiveBuffer = new byte[socket.ReceiveBufferSize];

            socket.BeginReceive(
                receiveBuffer, 
                0,
                receiveBuffer.Length, 
                SocketFlags.None, 
                DataReceived, 
                socket);
        }

        private void DataReceived(IAsyncResult ar)
        {
            var receivedMessage = CommunicationUtility.Deserialize(receiveBuffer);
            Console.WriteLine(receivedMessage.Message);
            socket.EndReceive(ar);
        }
    }
}
