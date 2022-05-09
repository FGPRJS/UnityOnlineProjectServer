using System;
using UnityOnlineProjectServer.Content;
using Xunit;

namespace Server.Tests
{
    public class FieldMapTest
    {
        [Theory]
        [InlineData(100.0f, 120.0f, 10, 12, 1)]
        public void ZRegionCountCheck(float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength)
        {
            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            Assert.True(map.regions.GetLength(0) == zRegionCount);
        }

        [Theory]
        [InlineData(100.0f, 120.0f, 10, 12, 1)]
        public void XRegionCountCheck(float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength)
        {
            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            Assert.True(map.regions.GetLength(1) == xRegionCount);
        }
    }
}
