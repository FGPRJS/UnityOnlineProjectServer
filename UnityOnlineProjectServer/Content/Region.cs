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
        public readonly float Width;
        public readonly float Height;

        private ConcurrentDictionary<GameObject, byte> _gameobjectInRegion;
        private ConcurrentDictionary<Region, byte> _nearbyRegions;

        public EventHandler<GameObject> GameObjectEnterEvent;
        public EventHandler<GameObject> GameObjectLostEvent;
        

        public Region(int regionID, float x, float z, float width, float height)
        {
            _x = x;
            _z = z;

            RegionId = regionID;

            Width = width;
            Height = height;
            _gameobjectInRegion = new ConcurrentDictionary<GameObject, byte>();
            _nearbyRegions = new ConcurrentDictionary<Region, byte>();
        }

        public List<GameObject> GetGameObjects()
        {
            var list = new List<GameObject>();
            foreach(var obj in _gameobjectInRegion.Keys)
            {
                list.Add(obj);
            }
            return list;
        }

        public List<GameObject> GetAllNearbyGameObjects()
        {
            var result = new List<GameObject>();
            foreach(var region in _nearbyRegions.Keys)
            {
                var objs = region.GetGameObjects();

                result.AddRange(objs);
            }
            return result;
        }

        public void AddNearbyRegion(Region nearbyRegion)
        {
            _nearbyRegions.TryAdd(nearbyRegion, 0);
            nearbyRegion.GameObjectLostEvent += isGameObjectLostToo;
        }

        private void isGameObjectLostToo(object sender, GameObject e)
        {
            if()
        }

        public void AddGameObject(GameObject obj)
        {
            if (isGameObjectInRegion(obj))
            {
                _gameobjectInRegion.TryAdd(obj, 0);
                obj.PositionChangedEvent += ObjPositionChangedEvent;
                GameObjectEnterEvent?.Invoke(this, obj);
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(obj));
            }
        }

        private void ObjPositionChangedEvent(object sender, System.Numerics.Vector3 e)
        {
            var obj = sender as GameObject;

            if (!isGameObjectInRegion(obj))
            {
                RemoveGameObject(obj);
            }
        }

        public void RemoveGameObject(GameObject obj)
        {
            byte dump;
            obj.PositionChangedEvent -= ObjPositionChangedEvent;
            bool result = _gameobjectInRegion.TryRemove(obj, out dump);
            if (result)
            {
                GameObjectLostEvent?.Invoke(this, obj);
            }
        }

        public bool isGameObjectInRegion(GameObject gameObject)
        {
            if((gameObject.Position.X >= _x) && (gameObject.Position.X < Width + _x)
                && (gameObject.Position.Z >= _z) && (gameObject.Position.Z < Height + _z))
            {
                return true;
            }

            return false;
        }

        public bool isNearbyRegion(Region region)
        {
            if (_nearbyRegions.ContainsKey(region))
            {
                return true;
            }
            return false;
        }
    }
}
