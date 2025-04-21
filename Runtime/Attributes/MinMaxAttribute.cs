using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Attributes
{
    public class MinMaxAttribute : PropertyAttribute
    {
        private float _min;
        private float _max;

        public float Min => _min;
        public float Max => _max;

        public MinMaxAttribute(float min, float max)
        {
            this.order = -1000;

            _min = min;
            _max = max;
        }
    }
}