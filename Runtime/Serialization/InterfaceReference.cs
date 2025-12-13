using System;
using UnityEngine;

namespace Moths.Serialization
{
    [System.Serializable]
    public abstract class InterfaceReference
    {
        [SerializeReference] protected object _object;

        public abstract Type GetInterfaceType();
    }


    [System.Serializable]
    public class InterfaceReference<T> : InterfaceReference
    {
        public T Value => _object == null ? default : (T)_object;

        public override Type GetInterfaceType() => typeof(T);

        public InterfaceReference(T value)
        {
            _object = value;
        }

        public static implicit operator T(InterfaceReference<T> reference) => reference.Value;
    }
}