using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Utils
{
    /// <summary>
    /// Generic Serializable Dictionary for Unity 2020.1.
    /// Simply declare your key/value types and you're good to go - zero boilerplate.
    /// </summary>
    [Serializable]
    public class GenericDictionary<TKey, TValue> : IDictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField, HideInInspector] private List<GenericPair> serializedList = new List<GenericPair>();

        [Serializable]
        public struct GenericPair
        {
            public TKey Key;
            public TValue Value;

            public GenericPair(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }

            public void SetKey(TKey key) => Key = key;
            public void SetValue(TValue value) => Value = value;
        }
        
        private Dictionary<TKey, TValue> _dictionary = new Dictionary<TKey, TValue>();
     
        public int Count => _dictionary.Count;
        
        public ICollection<TKey> Keys => _dictionary.Keys;
        
        public ICollection<TValue> Values => _dictionary.Values;
        
        public TValue this[TKey key]
        {
            get => _dictionary[key];

            set
            {
                _dictionary[key] = value;
                serializedList.Find(pair => EqualityComparer<TKey>.Default.Equals(pair.Key, key)).SetValue(value);
            }
        }
        
        public bool IsReadOnly { get; set; }
        
        public void OnBeforeSerialize() { }

        public void OnAfterDeserialize()
        {
            _dictionary.Clear();

            serializedList.ForEach(pair => _dictionary.Add(pair.Key, pair.Value));
        }
        
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _dictionary.GetEnumerator();
        
        IEnumerator IEnumerable.GetEnumerator() => _dictionary.GetEnumerator();

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            _dictionary.Add(item.Key, item.Value);
            serializedList.Add(new GenericPair(item.Key, item.Value));
        }

        public void Clear()
        {
            _dictionary.Clear();
            serializedList.Clear();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item) => _dictionary.Contains(item);

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentException("The array cannot be null.");
            if (arrayIndex < 0) throw new ArgumentOutOfRangeException(nameof(arrayIndex), "The starting array index cannot be negative.");
            if (array.Length - arrayIndex < _dictionary.Count) throw new ArgumentException("The destination array has fewer elements than the collection.");

            foreach (var pair in _dictionary)
            {
                array[arrayIndex] = pair;
                arrayIndex++;
            }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            if (_dictionary.Remove(item.Key))
            {
                serializedList.Remove(serializedList.Find(pair => EqualityComparer<TKey>.Default.Equals(pair.Key, item.Key)));

                return true;
            }

            return false;
        }
        
        public int RemoveAll(Predicate<GenericPair> predicate)
        {
            return serializedList.RemoveAll(predicate);
        }

        public void Add(TKey key, TValue value)
        {
            _dictionary.Add(key, value);
            serializedList.Add(new GenericPair(key, value));
        }

        public bool ContainsKey(TKey key) => _dictionary.ContainsKey(key);

        public bool Remove(TKey key)
        {
            if (_dictionary.Remove(key))
            {
                serializedList.Remove(serializedList.Find(pair => EqualityComparer<TKey>.Default.Equals(pair.Key, key)));

                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, out TValue value) => _dictionary.TryGetValue(key, out value);
        
        public static GenericDictionary<TKey, TValue> ToGenericDictionary(Dictionary<TKey, TValue> dictionary)
        {
            GenericDictionary<TKey, TValue> genericDictionary = new GenericDictionary<TKey, TValue>();
        
            foreach (TKey key in dictionary.Keys)
            {
                genericDictionary.Add(key, dictionary[key]);
            }
        
            return genericDictionary;
        }
    }
}