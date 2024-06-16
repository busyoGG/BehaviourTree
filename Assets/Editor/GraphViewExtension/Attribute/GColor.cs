using System;
using UnityEngine;

namespace GraphViewExtension
{
    [AttributeUsage(AttributeTargets.Field)]
    public class GColor: Attribute
    {
        private Color color;

        public GColor(float r,float g,float b,float a = 1)
        {
            color = new Color(r, g, b, a);
        }

        public Color GetColor()
        {
            return color;
        }
    }
}