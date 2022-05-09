using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using UnityOnlineProjectServer.Connection;

namespace UnityOnlineProjectServer.Content
{
    public class Region
    {
        public int RegionId { get; set; }

        private float _x;
        private float _z;
        private float _width;
        private float _height;

        private ConcurrentBag<GameObject> _gameobjectInRegion;
        private ConcurrentBag<Region> _nearbyRegions;

        public EventHandler<GameObject> GameObjectEnterEvent;
        public EventHandler<GameObject> GameObjectLostEvent;
        

        public Region(int regionID, float x, float z, float width, float height)
        {
            _x = x;
            _z = z;

            RegionId = regionID;

            _width = width;
            _height = height;
            _gameobjectInRegion = new ConcurrentBag<GameObject>();
            _nearbyRegions = new ConcurrentBag<Region>();
        }

        public void AddNearbyRegion(Region nearbyRegion)
        {
            _nearbyRegions.Add(nearbyRegion);
        }

        private void EnterRegionEventAction(object sender, GameObject entered)
        {
            
        }

        public bool isGameObjectInRegion(GameObject gameObject)
        {
            if((gameObject.Position.X >= _x) && (gameObject.Position.X < _width + _x)
                && (gameObject.Position.Z >= _z) && (gameObject.Position.X < _height + _z))
            {
                return true;
            }

            return false;
        }
    }
}
