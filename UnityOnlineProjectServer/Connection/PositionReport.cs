using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Connection
{
    public class PositionReport : TickTask
    {
        public PositionReport()
        {
            _interval = 3000;
            _maxCount = 0;
        }

    }
}
