using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Protocol;

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
            
            BeginReceive();
        }

        void BeginReceive()
        {
            try
            {
                ClientSocket.BeginReceive(
                receiveBuffer,
                0,
                receiveBuffer.Length,
                SocketFlags.None,
                DataReceivedCallback,
                ClientSocket);
            }
            catch(NullReferenceException ne)
            {
                Console.WriteLine("ClientSocket Lost");
            }
            catch(SocketException se)
            {
                Console.WriteLine("Socket is not available. Shutdown Client.");
                ShutDownRequest();
            }
        }

        private void DataReceivedCallback(IAsyncResult ar)
        {
            try
            {
                int bytesRead = ClientSocket.EndReceive(ar);

                if(bytesRead > 0)
                {
                    var receivedMessage = CommunicationUtility.Deserialize(receiveBuffer);
                    Console.WriteLine(receivedMessage?.Message);

                    heartbeat.ResetHeartbeat();
                }
                
                BeginReceive();
            }
            catch (NullReferenceException)
            {
                Console.WriteLine("ClientSocket Lost");
            }
            catch (SocketException)
            {
                Console.WriteLine("Socket is not available. Shutdown Client.");
                ShutDownRequest();
            }
        }

        #region Send Data
        internal void SendData(CommunicationMessage message)
        {
            try
            {
                var byteData = CommunicationUtility.Serialize(message);

                SendData(byteData);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot send Data. Reason : " + ex.Message);
            }
        }

        internal void SendData(byte[] byteData)
        {
            try
            {
                Console.WriteLine("Send data to client.");

                ClientSocket?.BeginSend(byteData, 0, byteData.Length, 0,
                    new AsyncCallback(SendCallback), ClientSocket);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot send Data. Reason : " + ex.Message);
            }
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
