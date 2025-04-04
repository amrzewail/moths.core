using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Fields
{
    public static class ExtensionMethods
    {
        public static bool Contains<TValue>(this IEnumerable<IGenericReference<TValue, IGenericField<TValue>, IGenericMonoBehaviour<TValue>>> enumerable, TValue val)
        {
            if (enumerable is IList list)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    var element = list[i] as IGenericReference<TValue, IGenericField<TValue>, IGenericMonoBehaviour<TValue>>;
                    if (element.GetValue().Equals(val)) return true;
                }
            }
            else
            {
                foreach (var element in enumerable)
                {
                    if (element.GetValue().Equals(val)) return true;
                }
            }
            return false;
        }
    }
}