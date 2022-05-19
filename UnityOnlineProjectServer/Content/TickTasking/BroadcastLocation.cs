using System;
using System.Collections.Generic;
using System.Text;
using UnityOnlineProjectServer.Connection;

namespace UnityOnlineProjectServer.Content.TickTasking
{
    public class BroadcastLocation : TickTask
    {
        public BroadcastLocation()
        {
            _interval = 100;
            _maxCount = 999;
        }
    }
}
