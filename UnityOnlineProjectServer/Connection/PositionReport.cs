using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Connection
{
    internal class PositionReport : TickTask
    {
        internal PositionReport()
        {
            _interval = 3000;
            _maxCount = 0;
        }

    }
}
