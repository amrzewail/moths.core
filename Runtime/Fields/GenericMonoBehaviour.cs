using System;
using System.Collections.Generic;
using UnityEngine;

using Moths.Attributes;

namespace Moths.Fields
{
    public interface IGenericMonoBehaviour { }

    public interface IGenericMonoBehaviour<T> : IGenericMonoBehaviour
    {
        T Value { get; set; }
    }

    public class GenericMonoBehaviour<TValue> : MonoBehaviour, IGenericMonoBehaviour<TValue> 
    {
        [SerializeField] protected GenericReference<TValue, GenericField<TValue>, GenericMonoBehaviour<TValue>> value;

#if UNITY_EDITOR
        [Space]
        [Header("Stacktrace")]
        [SerializeField]
        [TextArea(3, 10)]
#if ODIN_INSPECTOR
    [Sirenix.OdinInspector.ReadOnly]
#endif //ODIN_INSPECTOR
        [PersistScriptableObjectField]
        private List<string> _stacktraces;
#endif //UNITY_EDITOR

        public event Action Changed;

        public TValue Value
        {
            get
            {
                if (value == null) return default;
                return value.Value;
            }
            set
            {
#if UNITY_EDITOR
                string stackTrace = $"Changed value to: {value}\n{StackTraceUtility.ExtractStackTrace()}";
                _stacktraces.Insert(0, stackTrace);
                if (_stacktraces.Count > 100) _stacktraces.RemoveAt(100);
#endif
                this.value.Value = value;
                Changed?.Invoke();
            }
        }

        protected virtual void OnValidate()
        {
            Changed?.Invoke();
        }

        public override string ToString() => value.ToString();


        public static implicit operator TValue(GenericMonoBehaviour<TValue> monoBehaviour) => monoBehaviour.Value;
    }
}