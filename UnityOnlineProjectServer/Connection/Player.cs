using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Protocol;

namespace UnityOnlineProjectServer.Connection
{
    internal class Player
    {
        public string PlayerName;
        public static Dictionary<string, Player> PlayerMap = new Dictionary<string, Player>();

        public delegate void SendMessageRequest(CommunicationMessage<Dictionary<string, string>> message);
        public event SendMessageRequest SendMessageRequestEvent;

        public async void ProcessMessage(CommunicationMessage<Dictionary<string, string>> message)
        {
            MessageType messageType;

            try
            {
                messageType = (MessageType)Enum.Parse(typeof(MessageType), message.header.MessageName);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot Process Message");
                return;
            }

            switch (messageType)
            {
                case MessageType.HeartBeatRequest:
                    //Just Reply
                    SendACKMessage(message);
                    break;
                
                case MessageType.LoginRequest:

                    //Duplicate Name
                    if (PlayerMap.ContainsKey(message.header.MessageName))
                    {
                        SendNACKMessage(message, "Already exist user that has same name.");
                        return;
                    }

                    PlayerName = message.header.MessageName;
                    message.header.ACK = (int)ACK.ACK;
                    
                    var workTask = Task.Run(() =>
                    {
                        SendMessageRequestEvent.Invoke(message);
                    });
                    await workTask;

                    break;
            }
        }

        async void SendNACKMessage(CommunicationMessage<Dictionary<string,string>> replyMessage, string reason)
        {
            replyMessage.header.ACK = (int)ACK.NACK;
            replyMessage.header.Reason = reason;
            var workTask = Task.Run(() =>
            {
                SendMessageRequestEvent.Invoke(replyMessage);
            });
            await workTask;
        }

        async void SendACKMessage(CommunicationMessage<Dictionary<string, string>> replyMessage)
        {
            replyMessage.header.ACK = (int)ACK.ACK;
            var workTask = Task.Run(() =>
            {
                SendMessageRequestEvent.Invoke(replyMessage);
            });
            await workTask;
        }
    }
}
