using UnityEngine;
namespace Moths.Collections
{
    [System.Serializable]
    public struct OverrideableProperty<TValue>
    {
        [SerializeField] bool _isOverriden;
        [SerializeField] TValue _value;
        [SerializeField] TValue _override;

        public bool IsOverriden => _isOverriden;
        public void Override(TValue value)
        {
            _isOverriden = true;
            _override = value;
        }

        public void RemoveOverride() => _isOverriden = false;

        public static implicit operator TValue(OverrideableProperty<TValue> property) => property.Value;

        public TValue Value
        {
            get
            {
                if (_isOverriden) return _override;
                return _value;
            }
        }
    }
}