using UnityEngine;
namespace Moths.Collections
{
    [System.Serializable]
    public struct OptionalProperty<TValue>
    {
        [SerializeField] bool _isEnabled;
        [SerializeField] TValue _value;

        public bool IsEnabled => _isEnabled;

        public TValue Value
        {
            get
            {
                return _value;
            }
        }

        public OptionalProperty(TValue value, bool isEnabled = false)
        {
            _isEnabled = isEnabled;
            _value = value;
        }

        public void Set(TValue value)
        {
            _isEnabled = true;
            _value = value;
        }

        public void Unset()
        {
            _isEnabled = false;
        }

        public static implicit operator bool(OptionalProperty<TValue> property) => property._isEnabled;
        public static implicit operator TValue(OptionalProperty<TValue> property) => property.Value;
    }
}