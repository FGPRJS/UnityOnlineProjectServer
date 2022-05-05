using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Protocol
{
    internal class CommunicationUtility
    {
        public static byte[] Serialize(CommunicationMessage<Dictionary<string, string>> message)
        {
            var data = JsonConvert.SerializeObject(message);

            return Encoding.ASCII.GetBytes(data);
        }

        public static CommunicationMessage<Dictionary<string, string>> Deserialize(string json)
        {
            return JsonConvert.DeserializeObject<CommunicationMessage<Dictionary<string, string>>>(json);
        }

        public static CommunicationMessage<Dictionary<string, string>> Deserialize(byte[] jsonByte)
        {
            var data = Encoding.ASCII.GetString(jsonByte);

            return JsonConvert.DeserializeObject<CommunicationMessage<Dictionary<string, string>>>(data);
        }
    }
}
