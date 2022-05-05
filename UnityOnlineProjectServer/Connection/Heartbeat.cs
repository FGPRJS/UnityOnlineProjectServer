using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Protocol;

namespace UnityOnlineProjectServer.Connection
{
    internal class Heartbeat
    {
        internal const int heartbeatInterval = 30000;
        internal const int heartbeatTimeout = 180000;

        internal int currentHeartbeatTime = 0;

        internal delegate void HeartbeatTimeOut();
        internal event HeartbeatTimeOut HeartbeatTimeOutEvent;

        internal delegate void HeartbeatTick();
        internal event HeartbeatTick HeartbeatTickEvent;

        internal static CommunicationMessage<Dictionary<string, string>> heartbeatMessage;
        internal static byte[] heartbeatMessageByteData;



        internal Heartbeat()
        {
            if ((heartbeatMessage != null) && (heartbeatMessageByteData != null)) return;

            heartbeatMessage = new CommunicationMessage<Dictionary<string, string>>()
            {
                header = new Header()
                {
                    MessageName = MessageType.HeartBeatRequest.ToString()
                }
            };

            heartbeatMessageByteData = CommunicationUtility.Serialize(heartbeatMessage);
        }

        internal async void CountHeartbeat(int interval)
        {
            currentHeartbeatTime += interval;

            //Instead of BeginInvoke
            var workTask = Task.Run(() =>
            {
                HeartbeatTickEvent.Invoke();
            });
            await workTask;

            if (currentHeartbeatTime >= heartbeatTimeout)
            {
                currentHeartbeatTime = 0;
                //Instead of BeginInvoke
                workTask = Task.Run(() =>
                {
                    HeartbeatTimeOutEvent.Invoke();
                });
                await workTask;
            }
        }

        internal void ResetHeartbeat()
        {
            currentHeartbeatTime = 0;
        }
    }
}
