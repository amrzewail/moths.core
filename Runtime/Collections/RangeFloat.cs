using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Collections
{

    [System.Serializable]
    public struct RangeFloat
    {
        public float min;
        public float max;

        public RangeFloat(float min, float max)
        {
            this.min = min;
            this.max = max;
        }

        public float Random()
        {
            return UnityEngine.Random.Range(min, max);
        }

        public bool Contains(float value, float errorRange = 0)
        {
            return (value >= min - errorRange) && (value <= max + errorRange);
        }
    }
}