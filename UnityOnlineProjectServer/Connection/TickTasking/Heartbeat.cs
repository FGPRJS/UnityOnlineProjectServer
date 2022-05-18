using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityOnlineProjectServer.Protocol;

namespace UnityOnlineProjectServer.Connection
{
    public class Heartbeat : TickTask
    {
        public Heartbeat()
        {
            _interval = 10000;
            _maxCount = 4;
            _hasTimeout = true;
        }
    }
}
