using UnityEngine;

namespace Moths.Utility
{
    public static class RandomUtility
    {
        public static bool Boolean()
        {
            return UnityEngine.Random.Range(0f, 1f) > 0.5f;
        }

        /// <summary>
        /// Generates a random number between 0f and 1f
        /// </summary>
        public static float Float01() => UnityEngine.Random.Range(0f, 1f);

        /// <summary>
        /// Generates -1 or 1 only
        /// </summary>
        public static int IntNP() => Boolean() ? 1 : -1;
    }
}