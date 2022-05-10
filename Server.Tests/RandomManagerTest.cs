using System;
using System.Collections.Generic;
using System.Text;
using UnityOnlineProjectServer.Content;
using Xunit;

namespace Server.Tests
{
    public class RandomManagerTest
    {
        [Theory]
        [InlineData(100)]
        public void RandomInteger(int tryCount)
        {
            int minval = 0;
            int maxval = 5;

            for(var i = 0; i < tryCount; i++)
            {
                var result = RandomManager.GetIntegerRandom(minval, maxval);
                Assert.True(result >= minval);
                Assert.True(result < maxval);
            }
        }

        [Theory]
        [InlineData(100)]
        public void RandomDecimal(int tryCount)
        {
            int minval = 0;
            int maxval = 5;

            for (var i = 0; i < tryCount; i++)
            {
                var result = RandomManager.GetRandomWithfloatingPoint(minval, maxval);
                Assert.True(result >= minval);
                Assert.True(result < maxval);
            }
        }
    }
}
