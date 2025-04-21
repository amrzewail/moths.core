using UnityEngine;

namespace Moths.Extensions
{
    public static class VectorExtensions
    {
        public static Vector3 Parse(string value)
        {
            value = value.Substring(1, value.Length - 2);
            string[] vals = value.Split(',');
            return new Vector3(float.Parse(vals[0]), float.Parse(vals[1]), float.Parse(vals[2]));
        }

        public static float AxisToAngle(this Vector2 axis)
        {
            float angle = -180 * Mathf.Atan2(axis.y, axis.x) / Mathf.PI + 90;
            if (angle < 0) angle += 360;
            return angle;
        }
        public static float AxisToAngle(this Vector3 axis)
        {
            float angle = -180 * Mathf.Atan2(axis.y, axis.x) / Mathf.PI + 90;
            if (angle < 0) angle += 360;
            return angle;
        }

        public static Vector2 MapXZToXY(this Vector3 v)
        {
            return new Vector2(v.x, v.z);
        }

        public static Vector3 MapXYToXZ(this Vector2 v)
        {
            return new Vector3(v.x, 0, v.y);
        }
    }
}