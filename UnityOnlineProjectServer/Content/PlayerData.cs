using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace UnityOnlineProjectServer.Content
{
    internal class TankData : GameObject
    {
        public Quaternion TowerRotation;
        public Quaternion CannonRotation;

        public TankData(string _playerName)
        {
            PlayerName = _playerName;
        }
    }
}
