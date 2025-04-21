using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moths.Extensions
{
    public static class EnumerableExtensions
    {
        public static T Random<T>(this IEnumerable<T> enumerable)
        {
            int count = enumerable.Count();
            if (count == 0) return default(T);
            return enumerable.ElementAt(UnityEngine.Random.Range(0, count));
        }
    }
}
