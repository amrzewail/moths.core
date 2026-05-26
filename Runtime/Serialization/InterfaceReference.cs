using System;
using UnityEngine;

namespace Moths.Serialization
{
    [System.Serializable]
    public struct InterfaceReference<T>
    {
        [SerializeReference] private object _object;

        public T Value => _object == null ? default : (T)_object;

        public Type GetInterfaceType() => typeof(T);

        public InterfaceReference(T value)
        {
            _object = value;
        }

        public static implicit operator T(InterfaceReference<T> reference) => reference.Value;

        public static implicit operator bool(InterfaceReference<T> reference) => reference._object != null && reference.Value != null;
    }
}