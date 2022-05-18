using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Protocol
{
    public class CommunicationUtility
    {

        public static byte[] Serialize(CommunicationMessage<Dictionary<string, string>> message)
        {
            try
            {
                var data = JsonConvert.SerializeObject(message);

                return Encoding.UTF8.GetBytes(data);
            }
            catch (JsonSerializationException ex)
            {
                throw ex;
            }
        }

        public static CommunicationMessage<Dictionary<string, string>> Deserialize(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<CommunicationMessage<Dictionary<string, string>>>(json);
            }
            catch (JsonSerializationException ex)
            {
                throw ex;
            }
        }

        public static CommunicationMessage<Dictionary<string, string>> Deserialize(byte[] jsonByte)
        {
            try
            {
                var data = Encoding.UTF8.GetString(jsonByte);

                var result = JsonConvert.DeserializeObject<CommunicationMessage<Dictionary<string, string>>>(data);

                return result;
            }  
            catch(JsonSerializationException ex)
            {
                throw ex;
            }
        }
    }
}
