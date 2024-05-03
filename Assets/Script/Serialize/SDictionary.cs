using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SDictionary { }

[Serializable]
public class SDictionary<TKey, TValue> : SDictionary, ISerializationCallbackReceiver, IDictionary<TKey, TValue>
{
    [SerializeField] private List<SKeyValue<TKey,TValue>> list = new List<SKeyValue<TKey, TValue>>();


    private Dictionary<TKey, int> KeyPositions => _keyPositions.Value;
    private Lazy<Dictionary<TKey, int>> _keyPositions;

    public SDictionary()
    {
        _keyPositions = new Lazy<Dictionary<TKey, int>>(MakeKeyPositions);
    }

    private Dictionary<TKey, int> MakeKeyPositions()
    {
        var dictionary = new Dictionary<TKey, int>(list.Count);
        for (var i = 0; i < list.Count; i++)
        {
            dictionary[list[i].key] = i;
        }
        return dictionary;
    }

    public void OnBeforeSerialize() { }

    public void OnAfterDeserialize()
    {
        _keyPositions = new Lazy<Dictionary<TKey, int>>(MakeKeyPositions);
    }

    #region IDictionary<TKey, TValue>

    public TValue this[TKey key]
    {
        get => list[KeyPositions[key]].value;
        set
        {
            var pair = new SKeyValue<TKey,TValue>(key, value);
            if (KeyPositions.ContainsKey(key))
            {
                list[KeyPositions[key]] = pair;
            }
            else
            {
                KeyPositions[key] = list.Count;
                list.Add(pair);
            }
        }
    }

    public ICollection<TKey> Keys => list.Select(tuple => tuple.key).ToArray();
    public ICollection<TValue> Values => list.Select(tuple => tuple.value).ToArray();

    public void Add(TKey key, TValue value)
    {
        if (KeyPositions.ContainsKey(key))
            throw new ArgumentException("An element with the same key already exists in the dictionary.");
        else
        {
            KeyPositions[key] = list.Count;
            list.Add(new SKeyValue<TKey, TValue>(key, value));
        }
    }

    public bool ContainsKey(TKey key) => KeyPositions.ContainsKey(key);

    public bool Remove(TKey key)
    {
        if (KeyPositions.TryGetValue(key, out var index))
        {
            KeyPositions.Remove(key);

            list.RemoveAt(index);
            for (var i = index; i < list.Count; i++)
                KeyPositions[list[i].key] = i;

            return true;
        }
        else
            return false;
    }

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (KeyPositions.TryGetValue(key, out var index))
        {
            value = list[index].value;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    #endregion

    #region ICollection <KeyValuePair<TKey, TValue>>

    public int Count => list.Count;
    public bool IsReadOnly => false;

    public void Add(KeyValuePair<TKey, TValue> kvp) => Add(kvp.Key, kvp.Value);

    public void Clear() => list.Clear();
    public bool Contains(KeyValuePair<TKey, TValue> kvp) => KeyPositions.ContainsKey(kvp.Key);

    public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        var numKeys = list.Count;
        if (array.Length - arrayIndex < numKeys)
            throw new ArgumentException("arrayIndex");
        for (var i = 0; i < numKeys; i++, arrayIndex++)
        {
            var entry = list[i];
            array[arrayIndex] = new KeyValuePair<TKey, TValue>(entry.key, entry.value);
        }
    }

    public bool Remove(KeyValuePair<TKey, TValue> kvp) => Remove(kvp.Key);

    public Dictionary<TKey, TValue> ToDictionary()
    {
        Dictionary<TKey, TValue> dic = new Dictionary<TKey, TValue>();
        foreach(var kvp in list)
        {
            dic.Add(kvp.key, kvp.value);
        }
        return dic;
    }

    #endregion

    #region IEnumerable <KeyValuePair<TKey, TValue>>

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return list.Select(ToKeyValuePair).GetEnumerator();

        static KeyValuePair<TKey, TValue> ToKeyValuePair(SKeyValue<TKey, TValue> skvp)
        {
            return new KeyValuePair<TKey, TValue>(skvp.key, skvp.value);
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    #endregion
}
