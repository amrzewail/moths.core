using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Moths.Fields
{
    public enum Type
    {
        Value,
        Field,
        MonoBehaviour,
    }

    public interface IGenericReference { }
    public class GenericReference : IGenericReference { }

    public interface IGenericReference<out TValue, out TField, out TMonoBehaviour>
    {
        TValue GetValue();
    };

    [System.Serializable]
    public class GenericReference<TValue, TField, TMonoBehaviour> : GenericReference, IGenericReference<TValue, TField, TMonoBehaviour>
        where TField : GenericField<TValue>
        where TMonoBehaviour : GenericMonoBehaviour<TValue>
    {
        [SerializeField] TValue value;
        [SerializeField] TField field;
        [SerializeField] TMonoBehaviour monoBehaviour;
        [SerializeField] Type valueType;

        public Type ValueType => valueType;

        public static implicit operator TValue(GenericReference<TValue, TField, TMonoBehaviour> p)
        {
            return p.Value;
        }

        public static bool operator ==(GenericReference<TValue, TField, TMonoBehaviour> reference, TValue value)
        {
            return reference.Value.Equals(value);
        }

        public static bool operator !=(GenericReference<TValue, TField, TMonoBehaviour> reference, TValue value)
        {
            return !reference.Value.Equals(value);
        }

        public override bool Equals(object obj)
        {
            return Value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }

        public TValue Value
        {
            get
            {
                return valueType switch
                {
                    Type.Value => value,
                    Type.Field => field,
                    Type.MonoBehaviour => monoBehaviour,
                    _ => default,
                };
            }
            set
            {
                switch (valueType)
                {
                    case Type.Value:
                        this.value = value;
                        break;
                    case Type.Field:
                        this.field.Value = value;
                        break;
                    case Type.MonoBehaviour:
                        this.monoBehaviour.Value = value;
                        break;
                }
            }
        }

        public TField Field
        {
            get
            {
                if (valueType == Type.Field) return field;
                return null;
            }
        }

        public TValue GetValue() => Value;
    }
}