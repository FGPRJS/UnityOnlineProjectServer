using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using UnityOnlineProjectServer.Connection;
using UnityOnlineProjectServer.Protocol;

namespace UnityOnlineProjectServer.Content
{
    public abstract class GameObject
    {
        public long id;
        public string GameObjectName;

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


        public GameObject()
        {
            //Position Report
            positionReport = new PositionReport();
            positionReport.TickEvent += TickEventAction;
        }

        ~GameObject()
        {
            positionReport.TickEvent -= TickEventAction;
        }

        void TickEventAction(object sender, EventArgs arg)
        {
            var somedata = CreateTickMessage();
            SendMessageRequestEvent?.Invoke(sender, somedata);
        }

        protected abstract CommunicationMessage<Dictionary<string, string>> CreateTickMessage();

        protected void SendMessage(object sender, CommunicationMessage<Dictionary<string, string>> message)
        {
            SendMessageRequestEvent?.Invoke(sender, message);
        }
    }
}
