using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SerializableDictionary<TKey, TValue>
{
    [SerializeField]
    private List<TKey> keys = new List<TKey>();

    [SerializeField]
    private List<TValue> values = new List<TValue>();

    public Dictionary<TKey, TValue> dictionary = new Dictionary<TKey, TValue>();

    public int Count { get => keys.Count;  }

    public void Add(TKey key, TValue value)
    {
        if (!dictionary.ContainsKey(key))
        {
            keys.Add(key);
            values.Add(value);
            dictionary.Add(key, value);
        }
    }

    public bool TryGetValue(TKey key, out TValue value)     => dictionary.TryGetValue(key, out value);

    public bool ContainsKey(TKey key)                       => dictionary.ContainsKey(key);

    public bool Remove(TKey key)
    {
        if (dictionary.ContainsKey(key))
        {
            int index = keys.IndexOf(key);
            keys.RemoveAt(index);
            values.RemoveAt(index);
            dictionary.Remove(key);
            return true;
        }
        return false;
    }

    public List<TKey>       GetKeys()   => keys;

    public List<TValue>     GetValues() => values;

    // Indexer for convenient access by key
    public TValue this[TKey key]
    {
        get
        {
            if (dictionary.ContainsKey(key))
            {
                return dictionary[key];
            }
            throw new KeyNotFoundException("The key does not exist in the dictionary.");
        }
        set
        {
            if (dictionary.ContainsKey(key))
            {
                dictionary[key] = value;
                int index = keys.IndexOf(key);
                values[index] = value;
            }
            else
            {
                Add(key, value);
            }
        }
    }
}
