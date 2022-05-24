using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Server.Tests
{
    public class DateTimeTest
    {
        [Fact]
        public async void TimeDataTypeTest()
        {
            DateTime dt1 = DateTime.Now;
            await Task.Delay(300);
            DateTime dt2 = DateTime.Now;

            var descGap = dt2 - dt1;
            var gapSeconds = descGap.Seconds;
            var gapMilliSeconds = descGap.Milliseconds;
            Assert.True (Math.Abs((gapSeconds - 300) / 1000) < 0.05 );
        }
    }
}
