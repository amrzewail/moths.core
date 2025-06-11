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
    public class GenericReference : IGenericReference 
    {
        public virtual void OnValidate() { }
    }

    public interface IGenericReference<TValue>
    {
        TValue Value { get; set; }
    }

    public interface IGenericReference<TValue, out TField, out TMonoBehaviour> : IGenericReference<TValue>
    {
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

        public Type ValueType { get => valueType; }


        public static implicit operator TValue(GenericReference<TValue, TField, TMonoBehaviour> p)
        {
            return p.Value;
        }

        public static bool operator ==(GenericReference<TValue, TField, TMonoBehaviour> reference, TValue value)
        {
            if (reference == null) return value == null;
            if (value == null) return reference.Value == null;
            return reference.Value.Equals(value);
        }

        public static bool operator !=(GenericReference<TValue, TField, TMonoBehaviour> reference, TValue value)
        {
            return !(reference == value);
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

        /// <summary>
        /// Auto sets ValueType to Field and assigns the value to field property.
        /// </summary>
        public TField Field
        {
            get
            {
                if (valueType == Type.Field) return field;
                return null;
            }
            set
            {
                valueType = Type.Field;
                field = value;
            }
        }
    }
}