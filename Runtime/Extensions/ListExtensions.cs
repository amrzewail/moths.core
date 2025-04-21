using System;
using System.Collections.Generic;

namespace Moths.Extensions
{
    public static class ListExtensions
    {
        public static bool Any<T>(this IList<T> list, Func<T, bool> predicate)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (predicate(list[i])) return true;
            }
            return false;
        }
    }
}