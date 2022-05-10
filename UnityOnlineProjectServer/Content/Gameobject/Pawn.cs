using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using UnityOnlineProjectServer.Connection;
using UnityOnlineProjectServer.Connection.TickTasking;
using UnityOnlineProjectServer.Protocol;
using UnityOnlineProjectServer.Utility;

namespace UnityOnlineProjectServer.Content
{
    public abstract class Pawn
    {
        public enum PawnType
        {
            Dummy,
            Tank
        }

        public long id;
        public string PawnName;

        public bool isDetector;
        public float sight;

        private Vector3 position;
        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                PositionChangedEvent?.Invoke(this, position);
            }
        }
        public Quaternion Rotation;

        public PositionReport positionReport;

        public event EventHandler<Vector3> PositionChangedEvent;
        public event EventHandler<CommunicationMessage<Dictionary<string, string>>> SendMessageRequestEvent;

        public Pawn(long id)
        {
            //ID
            this.id = id;

            //Position Report
            positionReport = new PositionReport();
            positionReport.TickEvent += TickEventAction;

            //Subscribe myself
            PositionChangedEvent += CurrentPositionChangedEvent;

            //NearbyObjects
            _nearbyObjects = new ConcurrentDictionary<Pawn, byte>();
        }

        


        //Changed position itself
        private void CurrentPositionChangedEvent(object sender, Vector3 e)
        {
            if (isDetector)
            {
                foreach (var target in _nearbyObjects.Keys)
                {
                    if (!isInSight(target))
                    {
                        RemovePawnInSight(target);
                    }
                }
            }
        }

        ~Pawn()
        {
            positionReport.TickEvent -= TickEventAction;
            PositionChangedEvent -= CurrentPositionChangedEvent;
        }

        void TickEventAction(object sender, EventArgs arg)
        {
            var somedata = CreateCurrentStatusMessage(MessageType.PawnPositionReport);
            SendMessageRequestEvent?.Invoke(sender, somedata);
        }

        public abstract CommunicationMessage<Dictionary<string, string>> CreateCurrentStatusMessage(MessageType messageType);

        public abstract void ApplyCurrentStatusMessage(CommunicationMessage<Dictionary<string, string>> message);

        protected void SendMessage(object sender, CommunicationMessage<Dictionary<string, string>> message)
        {
            SendMessageRequestEvent?.Invoke(sender, message);
        }

        #region Sight
        private ConcurrentDictionary<Pawn, byte> _nearbyObjects;

        public bool isInSight(Pawn other)
        {
            var XZPow = Math.Pow(other.Position.X - Position.X, 2) + Math.Pow(other.Position.Z - Position.Z, 2);
            if (XZPow < Math.Pow(sight, 2))
            {
                return true;
            }
            return false;
        }

        public void AddPawnInSight(Pawn obj)
        {
            if (!isDetector) return;
            if (_nearbyObjects.ContainsKey(obj)) return;

            _nearbyObjects.TryAdd(obj, (byte)0);

            var message = obj.CreateCurrentStatusMessage(MessageType.PawnSpawnReport);
            SendMessageRequestEvent?.Invoke(this, message);
        }

        public void RemovePawnInSight(Pawn obj)
        {
            if (!isDetector) return;
            if (!_nearbyObjects.ContainsKey(obj)) return;

            byte dummy;
            _nearbyObjects.TryRemove(obj, out dummy);

            var message = obj.CreateCurrentStatusMessage(MessageType.PawnDestroyReport);
            SendMessageRequestEvent?.Invoke(this, message);
        }

        public ICollection<Pawn> GetNearbyObjects()
        {
            var nearbyObjects = _nearbyObjects.Keys;
            return nearbyObjects;
        }
        #endregion

    }
}
