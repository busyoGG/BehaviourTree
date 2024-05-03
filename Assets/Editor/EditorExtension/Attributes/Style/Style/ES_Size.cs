

using System;
using UnityEngine;

public enum ESPercent
{
    None,
    All,
    Width,
    Height
}

/// <summary>
/// UI大小 参数x：宽度；参数y：高度；参数percent：是否比例制
/// </summary>
public class ES_Size : Attribute
{
    /// <summary>
    /// 大小
    /// </summary>
    private readonly Vector2 _size;
    /// <summary>
    /// 是否比例制
    /// </summary>
    private readonly ESPercent _isPercent;

    public ES_Size(float x, float y, ESPercent percent = ESPercent.None)
    {
        _size.Set(x, y);
        _isPercent = percent;
    }

    public ESPercent GetSizeType()
    {
        return _isPercent;
    }

    public Vector2 GetSize()
    {
        return _size;
    }

    public float GetWidth()
    {
        return _size.x;
    }

    public float GetHeight()
    {
        return _size.y;
    }
}
