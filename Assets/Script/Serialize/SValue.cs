using System;
using UnityEngine.Serialization;

[Serializable]
public struct SValue<TValue>
{
    [FormerlySerializedAs("Value")] public TValue value;

    public SValue(TValue value)
    {
        this.value = value;
    }
}
