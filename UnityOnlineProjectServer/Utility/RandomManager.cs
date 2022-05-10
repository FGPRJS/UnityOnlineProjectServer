using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Content
{
    public class RandomManager
    {
        private static RandomManager instance;
        public static RandomManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new RandomManager();
                return instance;
            }
        }

        public Random random = new Random();

        public static float GetRandomWithfloatingPoint(int min, int max)
        {
            var intval = Instance.random.Next(min, max);
            var decimalval = Instance.random.NextDouble();

            float result = (float)(intval + decimalval);

            return result;
        }

        public static int GetIntegerRandom(int min, int max)
        {
            return Instance.random.Next(min, max);
        }
    }
}
