using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using UnityOnlineProjectServer.Utility;
using Xunit;

namespace Server.Tests
{
    public class VectorQuaternionTest
    {
        [Theory]
        [InlineData(123.45, 678.91, 234.56)]
        [InlineData(-123.45, -678.91, -234.56)]
        public void Vector3ToString(float x, float y, float z)
        {
            Vector3 newV3 = new Vector3(x, y, z);
            var newV3str = newV3.ToString();
            Assert.Contains(x.ToString(), newV3str);
        }

        [Theory]
        [InlineData(123.45, 678.91, 234.56)]
        [InlineData(-123.45, -678.91, -234.56)]
        public void ParseVector3(float x, float y, float z)
        {
            Vector3 newV3 = new Vector3(x, y, z);
            var newV3str = newV3.ToString();
            var parsedV3 = NumericParser.ParseVector(newV3str);
            Assert.Equal(newV3, parsedV3);
        }

        [Theory]
        [InlineData(123.45, 678.91, 234.56, 789.12)]
        [InlineData(-123.45, -678.91, -234.56, -789.12)]
        public void ParseQuaternion(float x, float y, float z, float w)
        {
            Quaternion newQ = new Quaternion(x, y, z, w);
            var newQstr = newQ.ToString();
            var parsedQ = NumericParser.ParseQuaternion(newQstr);
            Assert.Equal(newQ, parsedQ);
        }
    }
}
