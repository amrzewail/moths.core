using Moths.Attributes;
using System;
using UnityEngine;

namespace Moths.Collections
{
    [System.Serializable]
    public struct Unique : IEquatable<Unique>, IEquatable<long>
    {
        private static Unique _empty = new Unique();

        [SerializeField, ReadOnly] long _identifier;

        public override int GetHashCode() => _identifier.GetHashCode();

        public override bool Equals(object obj)
        {
            return obj is Unique unique && unique._identifier == _identifier;
        }

        public bool Equals(long other)
        {
            return _identifier == other;
        }

        public bool Equals(Unique other)
        {
            return _identifier == other._identifier;
        }

        public override string ToString() => _identifier.ToString();


        public static implicit operator long(Unique unique) => unique._identifier;
        public static bool operator == (Unique unique1, Unique unique2) => unique1.Equals(unique2);
        public static bool operator !=(Unique unique1, Unique unique2) => !unique1.Equals(unique2);

        public static Unique Empty => _empty;
    }
}