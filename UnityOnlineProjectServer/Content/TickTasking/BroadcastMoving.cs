using System;
using System.Collections.Generic;
using System.Text;
using UnityOnlineProjectServer.Connection;

namespace UnityOnlineProjectServer.Content.TickTasking
{
    public class BroadcastMoving : TickTask
    {
        public BroadcastMoving()
        {
            _interval = 100;
            _maxCount = 999;
        }
    }
}
