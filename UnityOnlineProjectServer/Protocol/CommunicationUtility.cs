using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Protocol
{
    internal class CommunicationUtility
    {
        public static byte[] Serialize(CommunicationMessage message)
        {
            var data = JsonConvert.SerializeObject(message);

            return Encoding.ASCII.GetBytes(data);
        }

        public static CommunicationMessage Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<CommunicationMessage>(json);
        }

        public static CommunicationMessage Deserialize(byte[] jsonByte)
        {
            var data = Encoding.ASCII.GetString(jsonByte);

            return JsonConvert.DeserializeObject<CommunicationMessage>(data);
        }
    }
}
