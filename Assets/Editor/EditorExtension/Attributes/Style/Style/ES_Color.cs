

using System;
using UnityEngine;

public class ES_Color : Attribute
{
    private readonly Color _color;

    public ES_Color(float r, float g, float b ,float a = 1)
    {
        _color = new Color(r, g, b, a);
    }

    public Color GetColor()
    {
        return _color;
    }
}
