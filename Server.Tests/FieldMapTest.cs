using System;
using System.Diagnostics;
using System.Numerics;
using System.Threading;
using UnityOnlineProjectServer.Connection;
using UnityOnlineProjectServer.Content;
using Xunit;

namespace Server.Tests
{
    public class FieldMapTest
    {
        [Theory]
        [InlineData(100.0f, 120.0f, 10, 12, 1)]
        public void ZRegionCountCheck
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength)
        {
            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            Assert.True(map.regions.GetLength(0) == zRegionCount);
        }

        [Theory]
        [InlineData(100.0f, 120.0f, 10, 12, 1)]
        public void XRegionCountCheck
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength)
        {
            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            Assert.True(map.regions.GetLength(1) == xRegionCount);
        }

        [Theory]
        [InlineData(100.0f, 120.0f, 10, 6, 1, 3, 3, 10.0f)]
        [InlineData(1000.0f, 120.0f, 1000, 6, 1, 3, 3, 1.0f)]
        public void WidthCheck
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength
            , int xIndex, int zIndex, float ExpectedWidth)
        {
            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            Assert.Equal(ExpectedWidth, map.regions[zIndex, xIndex].Width);
        }

        [Theory]
        [InlineData(100.0f, 120.0f, 10, 6, 1, 3, 3, 20.0f)]
        [InlineData(100.0f, 120.0f, 10, 120, 1, 3, 3, 1.0f)]
        public void HeightCheck
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength
            , int xIndex, int zIndex, float ExpectedHeight)
        {
            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            Assert.Equal(ExpectedHeight, map.regions[zIndex, xIndex].Height);
        }

        [Theory]
        [InlineData(100.0f, 120.0f, 10, 6, 1, 3, 3)]
        public void BorderCheck_IN
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength
            , int xIndex, int zIndex)
        {
            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            var sampleRegion = map.regions[zIndex, xIndex];

            Tank obj = new Tank();
            obj.Position = new Vector3(sampleRegion.Width * xIndex, 0, sampleRegion.Height * zIndex);

            Assert.True(sampleRegion.isGameObjectInRegion(obj));
        }

        [Theory]
        [InlineData(100.0f, 60.0f, 10, 12, 1, 3, 3)]
        public void BorderCheck_OUT
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength
            ,int xIndex, int zIndex)
        {
            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            var sampleRegion = map.regions[zIndex, xIndex];

            Tank obj = new Tank();
            obj.Position = new Vector3(sampleRegion.Width * (xIndex + 1), 0, sampleRegion.Height * (zIndex + 1));

            Assert.False(sampleRegion.isGameObjectInRegion(obj));
        }

        [Theory]
        [InlineData(100.0f, 100.0f, 10, 10, 1, 3, 3, 4, 3)]
        [InlineData(100.0f, 100.0f, 10, 10, 1, 3, 3, 3, 4)]
        [InlineData(100.0f, 100.0f, 10, 10, 1, 3, 3, 2, 3)]
        [InlineData(100.0f, 100.0f, 10, 10, 1, 3, 3, 2, 2)]
        [InlineData(100.0f, 100.0f, 10, 10, 1, 3, 3, 4, 4)]
        [InlineData(100.0f, 100.0f, 10, 10, 2, 3, 3, 5, 5)]
        [InlineData(100.0f, 100.0f, 10, 10, 2, 3, 3, 1, 1)]
        public void NearbyRegion_True
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength
            , int xIndex, int zIndex, int otherRegionXIndex, int otherRegionZIndex)
        {
            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            var sampleRegion = map.regions[zIndex, xIndex];

            var otherRegion = map.regions[otherRegionZIndex, otherRegionXIndex];

            Assert.True(sampleRegion.isNearbyRegion(otherRegion));
        }

        [Theory]
        [InlineData(100.0f, 100.0f, 10, 10, 1, 3, 3, 5, 3)]
        [InlineData(100.0f, 100.0f, 10, 10, 1, 3, 3, 3, 5)]
        [InlineData(100.0f, 100.0f, 10, 10, 1, 3, 3, 1, 3)]
        [InlineData(100.0f, 100.0f, 10, 10, 1, 3, 3, 3, 1)]
        [InlineData(100.0f, 100.0f, 10, 10, 2, 3, 3, 6, 3)]
        [InlineData(100.0f, 100.0f, 10, 10, 2, 3, 3, 3, 3)] //Do Not contain itself as nearbyregion
        public void NearbyRegion_False
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength
            , int xIndex, int zIndex, int otherRegionXIndex, int otherRegionZIndex)
        {
            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            var sampleRegion = map.regions[zIndex, xIndex];
            var otherRegion = map.regions[otherRegionZIndex, otherRegionXIndex];

            Assert.False(sampleRegion.isNearbyRegion(otherRegion));
        }

        [Theory]
        [InlineData(100.0f, 100.0f, 10, 10, 2, 3, 3)]
        public void FindObjectsAppropriateRegion
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength
            , int xIndex, int zIndex)
        {
            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            var sampleRegion = map.regions[zIndex, xIndex];

            Tank obj = new Tank();
            obj.Position = new Vector3(sampleRegion.Width * xIndex, 0, sampleRegion.Height * zIndex);

            var appropriateRegion = map.GetAppropriateRegion(obj);

            Assert.Equal(sampleRegion, appropriateRegion);
        }

        [Theory]
        [InlineData(100.0f, 100.0f, 10, 10, 2)]
        public void FindObjectsAppropriateRegionOutofRange
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength)
        {
            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            Tank obj = new Tank();
            obj.Position = new Vector3(xSize + 1, 0, zSize + 1);

            var appropriateRegion = map.GetAppropriateRegion(obj);

            Assert.Null(appropriateRegion);
        }

        [Theory]
        [InlineData(100.0f, 100.0f, 10, 10, 2, 3, 3)]
        public void ListenGameObjectEnterEvent
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength
            , int xIndex, int zIndex)
        {
            AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

            var sw = new Stopwatch();
            sw.Start();

            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            var sampleRegion = map.regions[zIndex, xIndex];

            sampleRegion.GameObjectEnterEvent += (sender, obj) =>
            {
                _autoResetEvent.Set();
            };

            Tank obj = new Tank();
            obj.Position = new Vector3(sampleRegion.Width * xIndex, 0, sampleRegion.Height * zIndex);

            var appropriateRegion = map.GetAppropriateRegion(obj);

            appropriateRegion.AddGameObject(obj);

            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds < 500);
            Assert.True(_autoResetEvent.WaitOne());
        }

        [Theory]
        [InlineData(100.0f, 100.0f, 10, 10, 2, 3, 3)]
        public void ListenGameObjectLostEvent
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength
            , int xIndex, int zIndex)
        {
            AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

            var sw = new Stopwatch();
            sw.Start();

            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            var sampleRegion = map.regions[zIndex, xIndex];

            sampleRegion.GameObjectLostEvent += (sender, obj) =>
            {
                _autoResetEvent.Set();
            };

            Tank obj = new Tank();
            obj.Position = new Vector3(sampleRegion.Width * xIndex, 0, sampleRegion.Height * zIndex);

            map.GetAppropriateRegion(obj).AddGameObject(obj);

            obj.Position = new Vector3(sampleRegion.Width * ++xIndex, 0, sampleRegion.Height * ++zIndex);

            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds < 500);
            Assert.True(_autoResetEvent.WaitOne());
        }

        [Theory]
        [InlineData(100.0f, 100.0f, 10, 10, 2, 3, 3, 6, 3)]
        public void ListenOtherRegionEnterEvent
            (float xSize, float zSize, int xRegionCount, int zRegionCount, int nearbyRegionLength
            , int xIndex, int zIndex, int otherRegionXIndex, int otherRegionZIndex)
        {
            AutoResetEvent _autoResetEvent = new AutoResetEvent(false);


            var sw = new Stopwatch();
            sw.Start();

            FieldMap map = new FieldMap(xSize, zSize, xRegionCount, zRegionCount, nearbyRegionLength);

            var sampleRegion = map.regions[zIndex, xIndex];
            var otherRegion = map.regions[otherRegionZIndex, otherRegionXIndex];

            otherRegion.GameObjectEnterEvent += (sender, obj) =>
            {
                _autoResetEvent.Set();
            };

            Tank obj = new Tank();
            obj.Position = new Vector3(sampleRegion.Width * xIndex, 0, sampleRegion.Height * zIndex);

            map.GetAppropriateRegion(obj).AddGameObject(obj);

            obj.Position = new Vector3(otherRegion.Width * otherRegionXIndex, 0, otherRegion.Height * otherRegionZIndex);

            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds < 500);
            Assert.True(_autoResetEvent.WaitOne());
        }
    }
}
