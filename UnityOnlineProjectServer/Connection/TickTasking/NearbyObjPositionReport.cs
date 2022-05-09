using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Connection.TickTasking
{
    public class NearbyObjPositionReport : TickTask
    {
        public NearbyObjPositionReport()
        {
            _interval = 100;
            _maxCount = 0;
        }
    }
}
