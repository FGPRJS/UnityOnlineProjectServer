using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;

namespace UnityOnlineProjectServer.Utility
{
    internal class NumericParser
    {
        public static Vector3 ParseVector(string stringVector)
        {
            var regexVector = Regex.Replace(stringVector, "[^0-9,]", "");

            var parseVector = regexVector.Split(',');

            Vector3 newVector = new Vector3(
                float.Parse(parseVector[0]),
                float.Parse(parseVector[1]),
                float.Parse(parseVector[2]));

            return newVector;
        }

        public static Quaternion ParseQuaternion(string stringQuaternion)
        {
            var regexQuaternion = Regex.Replace(stringQuaternion, "[^0-9, ]", "");

            var parseQuaternion = regexQuaternion.Split(' ');

            Quaternion newQuaternion = new Quaternion(
                float.Parse(parseQuaternion[0]),
                float.Parse(parseQuaternion[1]),
                float.Parse(parseQuaternion[2]),
                float.Parse(parseQuaternion[3]));

            return newQuaternion;
        }
    }
}
