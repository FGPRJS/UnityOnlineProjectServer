using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using UnityOnlineProjectServer.Connection;
using UnityOnlineProjectServer.Content.Gameobject.Implements;
using UnityOnlineProjectServer.Utility;

namespace UnityOnlineProjectServer.Content.Map
{
    public class GameField
    {
        public long MapID;
        public bool isEnterable = true;

        public ConcurrentDictionary<GameObject, byte> detectors;
        public ConcurrentDictionary<GameObject, byte> nonDetectors;

        public long CurrentGameObjectID = 1;

        public GameField()
        {
            detectors = new ConcurrentDictionary<GameObject, byte>();
            nonDetectors = new ConcurrentDictionary<GameObject, byte>();
        }

        public GameObject CreateGameObject(GameObject.GameObjectType type, Vector3 position)
        {
            GameObject newObject = null;

            switch (type)
            {
                case GameObject.GameObjectType.Tank:

                    newObject = new Tank(CurrentGameObjectID++);
                    newObject.Position = position;

                    break;

                case GameObject.GameObjectType.Dummy:

                    newObject = new Dummy(CurrentGameObjectID++);
                    newObject.Position = position;

                    break;
            }

            newObject.PositionChangedEvent += GameObjectPositionChangedEvent;

            CheckEachOther(newObject);

            if (newObject.isDetector)
            {
                detectors.TryAdd(newObject, 0);
            }
            else
            {
                nonDetectors.TryAdd(newObject, 0);
            }

            return newObject;
        }

        private void GameObjectPositionChangedEvent(object sender, Vector3 e)
        {
            var target = sender as GameObject;

            CheckEachOther(target);
        }

        private void CheckEachOther(GameObject target)
        {
            //check each other detector
            foreach (var detector in detectors.Keys)
            {
                if (detector == target) continue;

                if (detector.isInSight(target))
                {
                    detector.AddGameObjectInSight(target);
                }
                else
                {
                    detector.RemoveGameObjectInSight(target);
                }

                if (target.isInSight(detector))
                {
                    target.AddGameObjectInSight(detector);
                }
                else
                {
                    target.RemoveGameObjectInSight(detector);
                }
            }
            //check non detector
            foreach (var nondetector in nonDetectors.Keys)
            {
                if (target.isInSight(nondetector))
                {
                    target.AddGameObjectInSight(nondetector);
                }
                else
                {
                    target.RemoveGameObjectInSight(nondetector);
                }
            }
        }

        public void RemoveGameObject(GameObject target)
        {
            byte dummy;

            if (target.isDetector)
            {
                detectors.TryRemove(target, out dummy);
            }
            else
            {
                nonDetectors.TryRemove(target, out dummy);
            }

            foreach (var detector in detectors.Keys)
            {
                detector.RemoveGameObjectInSight(target);
            }
        }
    }
}
