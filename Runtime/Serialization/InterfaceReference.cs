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

        [System.Serializable]
        private struct SerializationData
        {
            public string type;
            public string obj;
        }

        public string Serialize()
        {
            if (_object == null) return "{}";
            SerializationData data;
            data.type = _object.GetType().AssemblyQualifiedName;
            data.obj = JsonUtility.ToJson(_object);
            return JsonUtility.ToJson(data);
        }
        public void Deserialize(string json)
        {
            if (json == "{}")
            {
                _object = null;
                return;
            }
            SerializationData data = JsonUtility.FromJson<SerializationData>(json);
            _object = JsonUtility.FromJson(data.obj, Type.GetType(data.type));
        }
            
        public void Copy(InterfaceReference<T> copyFrom) => Deserialize(copyFrom.Serialize());
    }
}