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
    }
}
