using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Content.TickTasking
{
    public class BroadcastPosition : TickTask
    {
        public BroadcastPosition()
        {
            _interval = 500;
            _maxCount = 9999;
        }
    }
}
