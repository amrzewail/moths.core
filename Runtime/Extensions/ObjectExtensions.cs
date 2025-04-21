using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Moths.Extensions
{
    public static class ObjectExtensions
    {
        public static T FindInterfaceOfType<T>(this Object obj)
        {
            IEnumerable<T> objs = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<T>();
            if (objs != null && objs.Count() > 0) return objs.ElementAt(0);
            return default(T);
        }

        public static T[] FindInterfacesOfType<T>(this Object obj)
        {
            IEnumerable<T> objs = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<T>();
            if (objs != null) return objs.ToArray();
            return null;
        }
    }
}