using System;
using UnityEngine.Serialization;

[Serializable]
public struct SKeyValue<TKey,TValue>
{
    [FormerlySerializedAs("Key")] public TKey key;
    [FormerlySerializedAs("Value")] public TValue value;

    public SKeyValue(TKey key, TValue value)
    {
        this.key = key;
        this.value = value;
    }
}