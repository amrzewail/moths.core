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

        public float Clamp(float value) => Mathf.Clamp(value, min, max);

        public float Lerp(float t) => Mathf.Lerp(min, max, t);

        public static implicit operator RangeFloat((float min, float max) range)
        {
            return new RangeFloat(range.min, range.max);
        }
    }
}