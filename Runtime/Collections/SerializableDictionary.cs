using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Collections
{
    internal static class SerializableDictionary
    {
        internal static HashSet<object> DidInitialize;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Initialize()
        {
            DidInitialize = new();
        }
    }

    [System.Serializable]
    public class SerializableDictionary<TKey, TValue>
    {
        [System.Serializable]
        public struct KeyValuePair : IEquatable<KeyValuePair>
        {
            public TKey key;
            public TValue value;

            public override int GetHashCode()
            {
                return key.GetHashCode();
            }

            public bool Equals(KeyValuePair other)
            {
                return key.Equals(other.key);
            }

            public override bool Equals(object obj)
            {
                return obj is KeyValuePair other && Equals(other);
            }
        }

        [SerializeField] List<KeyValuePair> _pairs;

        private Dictionary<TKey, TValue> _dictionary = new();

        public TValue this[TKey key]
        {
            get
            {
                SyncDictionaryWithPairs(); 
                return _dictionary[key];
            }
            set
            {
                if (!Application.isPlaying)
                {
                    if (_pairs == null) _pairs = new();
                    var hashSet = _pairs.ToHashSet();
                    var pair = new KeyValuePair
                    {
                        key = key,
                        value = value,
                    };
                    if (hashSet.Contains(pair)) hashSet.Remove(pair);
                    hashSet.Add(pair);
                    _pairs = hashSet.ToList();
                    SyncDictionaryWithPairs();
                    return;
                }
                SyncDictionaryWithPairs();
                _dictionary[key] = value;
            }
        }

        public void Remove(TKey key)
        {
            if (!Application.isPlaying)
            {
                _pairs.Remove(new() { key = key });
                _dictionary.Clear();
                return;
            }
            _dictionary.Remove(key);
        }

        public void Clear()
        {
            if (_pairs == null) _pairs = new();
            if (_dictionary == null) _dictionary = new();
            _pairs.Clear();
            _dictionary.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            SyncDictionaryWithPairs();
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            SyncDictionaryWithPairs();
            return _dictionary.TryGetValue(key, out value);
        }

        public void UpdatePairsForSerialization()
        {
            _pairs.Clear();
            foreach (var pair in _dictionary) _pairs.Add(new() { key = pair.Key, value = pair.Value });
        }

        private void SyncDictionaryWithPairs()
        {
            if (Application.isPlaying)
            {
                if (SerializableDictionary.DidInitialize.Contains(this)) return;

                SerializableDictionary.DidInitialize.Add(this);
            }

        SYNC:

            if (_pairs == null) _pairs = new();

            foreach (var pair in _pairs) _dictionary[pair.key] = pair.value;
        }


        // --- ENUMERATOR IMPLEMENTATION START ---

        // Duck-typed struct enumerator for zero-allocation foreach loops
        public Enumerator GetEnumerator()
        {
            SyncDictionaryWithPairs();
            return new Enumerator(_dictionary);
        }

        public struct Enumerator : IEnumerator<KeyValuePair>
        {
            private Dictionary<TKey, TValue>.Enumerator _enumerator;

            internal Enumerator(Dictionary<TKey, TValue> dictionary)
            {
                _enumerator = dictionary.GetEnumerator();
            }

            // Convert standard System.Collections.Generic.KeyValuePair to your custom KeyValuePair
            public KeyValuePair Current
            {
                get
                {
                    var current = _enumerator.Current;
                    return new KeyValuePair { key = current.Key, value = current.Value };
                }
            }

            object IEnumerator.Current => Current;

            public bool MoveNext() => _enumerator.MoveNext();

            public void Reset() => ((IEnumerator)_enumerator).Reset();

            public void Dispose() => _enumerator.Dispose();
        }

        // --- ENUMERATOR IMPLEMENTATION END ---
    }
}