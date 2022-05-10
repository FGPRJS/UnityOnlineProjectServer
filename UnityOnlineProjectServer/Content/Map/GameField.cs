using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using UnityOnlineProjectServer.Connection;
using UnityOnlineProjectServer.Content.GameObject.Implements;
using UnityOnlineProjectServer.Utility;

namespace UnityOnlineProjectServer.Content.Map
{
    public class GameField
    {
        public long MapID;
        public bool isEnterable = true;

        public ConcurrentDictionary<Pawn, byte> detectors;
        public ConcurrentDictionary<Pawn, byte> nonDetectors;

        public long CurrentPawnID = 1;

        public GameField()
        {
            detectors = new ConcurrentDictionary<Pawn, byte>();
            nonDetectors = new ConcurrentDictionary<Pawn, byte>();
        }

        public Pawn CreatePawn(Pawn.PawnType type, Vector3 position)
        {
            Pawn newObject = null;

            switch (type)
            {
                case Pawn.PawnType.Tank:

                    newObject = new Tank(CurrentPawnID++);
                    newObject.Position = position;

                    break;

                case Pawn.PawnType.Dummy:

                    newObject = new Dummy(CurrentPawnID++);
                    newObject.Position = position;

                    break;
            }

            newObject.PositionChangedEvent += PawnPositionChangedEvent;

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

        private void PawnPositionChangedEvent(object sender, Vector3 e)
        {
            var target = sender as Pawn;

            CheckEachOther(target);
        }

        private void CheckEachOther(Pawn target)
        {
            //check each other detector
            foreach (var detector in detectors.Keys)
            {
                if (detector == target) continue;

                if (detector.isInSight(target))
                {
                    detector.AddPawnInSight(target);
                }
                else
                {
                    detector.RemovePawnInSight(target);
                }

                if (target.isInSight(detector))
                {
                    target.AddPawnInSight(detector);
                }
                else
                {
                    target.RemovePawnInSight(detector);
                }
            }
            //check non detector
            foreach (var nondetector in nonDetectors.Keys)
            {
                if (target.isInSight(nondetector))
                {
                    target.AddPawnInSight(nondetector);
                }
                else
                {
                    target.RemovePawnInSight(nondetector);
                }
            }
        }

        public void RemovePawn(Pawn target)
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
                detector.RemovePawnInSight(target);
            }
        }
    }
}
