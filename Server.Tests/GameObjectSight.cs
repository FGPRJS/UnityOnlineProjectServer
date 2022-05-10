using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using UnityOnlineProjectServer.Content;
using UnityOnlineProjectServer.Content.Map;
using Xunit;

namespace Server.Tests
{
    public class GameObjectSight
    {
        [Fact]
        public void AddTank()
        {
            GameField field = new GameField();

            field.CreateGameObject(UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank, new System.Numerics.Vector3(0,0,0));

            Assert.Single(field.detectors);
        }

        [Fact]
        public void CheckID_and_InSightCheck()
        {
            GameField field = new GameField();

            var newObj1 = field.CreateGameObject(UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank, new System.Numerics.Vector3(0, 0, 0));
            var newObj2 = field.CreateGameObject(UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank, new System.Numerics.Vector3(0, 0, 0));

            Assert.True(newObj2.id == 2);
            Assert.True(newObj1.GetNearbyObjects().Contains(newObj2));
            Assert.True(newObj2.GetNearbyObjects().Contains(newObj1));
        }

        [Fact]
        public void CheckMoveObjectOutofSight()
        {
            GameField field = new GameField();

            var newObj1 = field.CreateGameObject(
                UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank, 
                new System.Numerics.Vector3(0, 0, 0));
            var newObj2 = field.CreateGameObject(
                UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank, 
                new System.Numerics.Vector3(0, 0, 0));

            newObj1.Position = new System.Numerics.Vector3(newObj2.sight + 100, 0, newObj2.sight + 100);

            Assert.True(newObj1.GetNearbyObjects().Count == 0);
            Assert.True(newObj2.GetNearbyObjects().Count == 0);
        }

        [Fact]
        public void CheckMoveObjectInSight()
        {
            GameField field = new GameField();

            var newObj1 = field.CreateGameObject(UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank,
                new System.Numerics.Vector3(0, 0, 0));

            var newObj2 = field.CreateGameObject(UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank,
                new System.Numerics.Vector3(newObj1.sight + 999, 0, newObj1.sight + 999));
            newObj2.Position = newObj1.Position;


            Assert.True(newObj1.GetNearbyObjects().Count == 1);
            Assert.True(newObj2.GetNearbyObjects().Count == 1);
        }

        [Fact]
        public void CheckMoveObjectOutofSight3Objects()
        {
            GameField field = new GameField();

            var newObj1 = field.CreateGameObject(
                UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank,
                new System.Numerics.Vector3(0, 0, 0));
            var newObj2 = field.CreateGameObject(
                UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank,
                new System.Numerics.Vector3(newObj1.sight - 1, 0, 0));
            var newObj3 = field.CreateGameObject(
                UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank,
                new System.Numerics.Vector3(newObj1.sight - 1, 0, 0));

            newObj3.Position = new System.Numerics.Vector3(newObj1.sight + 1, 0, 0);

            Assert.Single(newObj1.GetNearbyObjects());
            Assert.Equal(2, newObj2.GetNearbyObjects().Count);
            Assert.Single(newObj3.GetNearbyObjects());
        }

        [Fact]
        public void CheckMoveObjectInSight3Objects()
        {
            GameField field = new GameField();

            var newObj1 = field.CreateGameObject(UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank,
                new System.Numerics.Vector3(0, 0, 0));
            var newObj2 = field.CreateGameObject(UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank,
                new System.Numerics.Vector3(newObj1.sight + 999, 0, newObj1.sight + 999));
            var newObj3 = field.CreateGameObject(UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank,
                new System.Numerics.Vector3(newObj1.sight + 999, 0, newObj1.sight + 999));

            newObj3.Position = newObj1.Position;


            Assert.True(newObj1.GetNearbyObjects().Count == 1);
            Assert.True(newObj2.GetNearbyObjects().Count == 0);
            Assert.True(newObj3.GetNearbyObjects().Count == 1);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(25)]
        [InlineData(50)]
        [InlineData(100)]
        public void MultipleObjectTime(int count)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            GameField field = new GameField();

            List<GameObject> objs = new List<GameObject>();

            for(int i = 0; i < count; i++)
            {
                var newObj = field.CreateGameObject(UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank,
                new System.Numerics.Vector3(0, 0, 0));

                objs.Add(newObj);
            }

            objs[objs.Count - 1].Position = new System.Numerics.Vector3(9999, 9999, 9999);

            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds < 15);
        }

        [Theory]
        [InlineData(10)]
        [InlineData(25)]
        [InlineData(50)]
        [InlineData(100)]
        public void MultipleObjectTimeWithNonDetectable(int count)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();

            GameField field = new GameField();

            for (int i = 0; i < count; i++)
            {
                field.CreateGameObject(UnityOnlineProjectServer.Content.GameObject.GameObjectType.Tank,
                new System.Numerics.Vector3(0, 0, 0));
            }

            var obj = field.CreateGameObject(GameObject.GameObjectType.Dummy,
                new System.Numerics.Vector3(999,9999,99999));
            obj.Position = new System.Numerics.Vector3(0, 0, 0);

            sw.Stop();

            Assert.True(sw.ElapsedMilliseconds < 15);
        }
    }
}
