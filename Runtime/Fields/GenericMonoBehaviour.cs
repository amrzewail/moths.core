using System;
using System.Collections.Generic;
using UnityEngine;

using Moths.Attributes;

namespace Moths.Fields
{
    public interface IGenericMonoBehaviour { }

    public interface IGenericMonoBehaviour<out T> : IGenericMonoBehaviour
    {
        T GetValue();
    }

    public class GenericMonoBehaviour<T> : MonoBehaviour, IGenericMonoBehaviour<T>
    {
        [SerializeField] protected T value;

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

        public T Value
        {
            get
            {
                return value;
            }
            set
            {
#if UNITY_EDITOR
                string stackTrace = $"Changed value to: {value}\n{StackTraceUtility.ExtractStackTrace()}";
                _stacktraces.Insert(0, stackTrace);
                if (_stacktraces.Count > 100) _stacktraces.RemoveAt(100);
#endif
                this.value = value;
                Changed?.Invoke();
            }
        }

        public T GetValue() => Value;

        protected virtual void OnValidate()
        {
            Changed?.Invoke();
        }

        public override string ToString() => value.ToString();


        public static implicit operator T(GenericMonoBehaviour<T> field) => field.Value;
    }
}