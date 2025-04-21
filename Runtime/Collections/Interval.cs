using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Collections
{
    public struct Interval
    {
        private float _elapsed;
        private float _interval;
        private bool _isEnabled;

        public Interval(float interval, bool instantStart = false)
        {
            _elapsed = 0;
            _interval = interval;
            _isEnabled = true;

            if (instantStart) _elapsed = _interval;
        }

        public bool Tick()
        {
            if (!_isEnabled) return false;

            _elapsed += Time.deltaTime;
            if (_elapsed >= _interval)
            {
                _elapsed = 0;
                return true;
            }
            return false;
        }

        public void Disable()
        {
            _isEnabled = false;
        }

        public void Reset(float interval)
        {
            _elapsed = 0;
            _interval = interval;
            _isEnabled = true;
        }
    }
}