using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityOnlineProjectProtocol;
using UnityOnlineProjectProtocol.Connection;
using UnityOnlineProjectProtocol.Protocol;

namespace UnityOnlineProjectServer.Connection
{
    internal class ConnectedClient
    {
        internal Socket ClientSocket;

        internal Heartbeat heartbeat;

        public readonly long id;
        private byte[] receiveBuffer;

        internal event ShutdownRequest ShutdownRequestEvent;
        internal delegate void ShutdownRequest(long id);

        internal ConnectedClient(long id)
        {
            this.id = id;
        }

        internal void Initialize(Socket socket)
        {
            this.ClientSocket = socket;
            socket.ReceiveBufferSize = AsyncStateObject.BufferSize;
            socket.SendBufferSize = AsyncStateObject.BufferSize;

            receiveBuffer = new byte[socket.ReceiveBufferSize];

            heartbeat = new Heartbeat();
            heartbeat.HeartbeatTimeOutEvent += () => { ShutDownRequest(); };
            heartbeat.HeartbeatTickEvent += () => { SendData(Heartbeat.heartbeatMessageByteData); };

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

            ClientSocket.EndReceive(ar);
        }

        #region Send Data
        internal void SendData(CommunicationMessage message)
        {
            var byteData = CommunicationUtility.Serialize(message);

            SendData(byteData);
        }

        internal void SendData(byte[] byteData)
        {
            Console.WriteLine("Send data to client.");

            ClientSocket.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), ClientSocket);
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
        #endregion

        internal async void ShutDownRequest()
        {
            var workTask = Task.Run(() =>
            {
                ShutdownRequestEvent.Invoke(id);
            });
            await workTask;
        }
    }
}
