using System;
using System.Collections.Generic;
using System.Text;

namespace UnityOnlineProjectServer.Connection
{
    internal abstract class TickTask
    {
        protected int _interval;
        protected int _maxCount;
        protected bool _hasTimeout = false;
        private int _currentElapsed = 0;
        private int _currentCount = 0;

        internal delegate void Tick();
        internal event Tick TickEvent; 
        
        internal delegate void Timeout();
        internal event Timeout TimeoutEvent;


        public void CountTick(int interval)
        {
            _currentElapsed += interval;

            if (_currentElapsed >= _interval)
            {
                _currentElapsed = 0;
                TickEvent?.Invoke();
                _currentCount++;
            }

            if (_hasTimeout && (_currentCount >= _maxCount))
            {
                _currentCount = 0;
                TimeoutEvent?.Invoke();
            }
        }

        public void ResetTimer()
        {
            _currentElapsed = 0;
            _currentCount = 0;
        }
    }
}
