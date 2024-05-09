﻿using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace BhTreeUtils
{
    [AttributeUsage(AttributeTargets.Field)]
    public class GWidth: Attribute
    {
        private Length _length;

        public GWidth(float width,LengthUnit type)
        {
            _length = new Length(width,type);
        }

        public Length GetLength()
        {
            return _length;
        }
    }
}