using System;
using UnityEngine;
using UnityEngine.Localization;

namespace Moths.Collections
{
    [System.Serializable]
    public struct LString : IEquatable<LString>, IEquatable<string>
    {
        [SerializeField] bool _useLocalization;
        [SerializeField] string _text;
        [SerializeField] LocalizedString _localization;

        public LString(string str)
        {
            this = default;
            _text = str;
        }

        public static implicit operator string(LString s)
        {
            if (s._useLocalization)
            {
                if (s._localization == null || s._localization.IsEmpty) return string.Empty;
                return s._localization.GetLocalizedString();
            }
            return s._text;
        }

        public static implicit operator bool(LString s) => !string.IsNullOrEmpty(s);

        public static bool operator ==(LString left, LString right) => (string)left == (string)right;
        public static bool operator !=(LString left, LString right) => !(left == right);
        public static bool operator ==(LString left, string right) => (string)left == right;
        public static bool operator !=(LString left, string right) => !(left == right);

        public override string ToString()
        {
            return (string)this;
        }

        public override bool Equals(object obj)
        {
            return obj is LString s && s == this;
        }

        public override int GetHashCode()
        {
            if (_useLocalization)
            {
                return _localization.GetHashCode();
            }
            else
            {
                return _text.GetHashCode();
            }
        }

        public bool Equals(LString other)
        {
            return other == this;
        }

        public bool Equals(string other)
        {
            return other == (string)this;
        }
    }
}