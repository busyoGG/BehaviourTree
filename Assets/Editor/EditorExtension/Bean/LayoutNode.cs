
using UnityEngine;
using System;
using System.Collections.Generic;

public struct UIListData
{
    public Action<GUIStyle, GUILayoutOption[]> action;
    public Func<GUIStyle> style;
    public Func<GUILayoutOption[]> options;
}

public class LayoutNode
{
    public readonly List<object> list = new List<object>();

    public Action layout;

    public LayoutNode Parent { get; private set; }

    public void AddUi(UIListData listData)
    {
        list.Add(listData);
    }

    public void AddUi(LayoutNode listData)
    {
        list.Add(listData);
        listData.Parent = this;
    }

    public void SetLayout(Action<GUIStyle, GUILayoutOption[]> layout, (Func<GUIStyle>, Func<GUILayoutOption[]>) layoutStyle)
    {
        this.layout = ()=> {
            layout(layoutStyle.Item1(),layoutStyle.Item2());
        };
        //_layoutStyle = layoutStyle;
    }
}
