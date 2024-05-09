using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class BaseEditor<T> : EditorWindow where T : EditorWindow
{
    private bool _isInited = false;

    private LayoutNode _root;

    private Type _type;

    private float _totalWidth;
    private float _curWidth;

    private Vector2 _scrollPosition = Vector2.zero;

    private readonly BindingFlags _flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly;


    public void OnEnable()
    {
        if (_isInited) return;

        //获得类型
        _type = GetType();

        //初始化窗口名称
        EName ename = _type.GetCustomAttribute<EName>();
        InitWindow(ename.GetName());

        //初始化UI
        Init();
    }

    private void InitWindow(string windowName)
    {
        T win = GetWindow<T>();
        win.titleContent = new GUIContent(windowName);
    }

    protected void RefreshUIInit()
    {
        _root = null;
        Init();
    }

    private void Init()
    {
        _root = new LayoutNode();

        //筛选包含编辑器拓展类型基类的对象
        var members = _type.GetMembers(_flag)
                            .Select(m => new
                            {
                                Member = m,
                                Attribute = m.GetCustomAttributes(typeof(EBase), false).FirstOrDefault() as EBase
                            })
                            .Where(x => x.Attribute != null)
                            .OrderBy(x => x.Attribute.lineNum);

        LayoutNode node = _root;
        //遍历对象，创建生成函数
        foreach (var item in members)
        {
            MemberInfo member = item.Member;
            var propAttrs = member.GetCustomAttributes(true);

            List<object> styles = new List<object>();
            object ui = null;

            //object layout = null;
            List<object> layouts = new List<object>();

            //遍历所有特性，分装到样式和ui和布局
            foreach (var prop in propAttrs)
            {
                Type attrType = prop.GetType();
                if (attrType.Name.IndexOf("ES_") != -1)
                {
                    styles.Add(prop);
                }
                else if (attrType.Name.IndexOf("E_") != -1)
                {
                    ui = prop;
                }
                else
                {
                    //layout = prop;
                    layouts.Add(prop);
                }
            }

            int back = 0;
            if (layouts.Count > 0)
            {
                foreach (var layout in layouts)
                {
                    dynamic layoutData = layout;

                    if (layoutData.IsStart())
                    {
                        LayoutNode next = new LayoutNode();

                        next.SetLayout(SetLayout(layout, next, member), (SetStyle(layout), SetOption(layout)));

                        node.AddUi(next);
                        node = next;

                        if (layoutData is EL_List)
                        {
                            if (layoutData.IsSingle())
                            {
                                back++;
                            }
                        }
                    }
                    else
                    {
                        //node = node.parent;
                        back++;
                    }
                }
            }


            //定义Name和GetVal
            Func<object> getVal;

            bool isField = member.MemberType == MemberTypes.Field;

            FieldInfo field = member as FieldInfo;

            //判断数据类型是否List
            bool isList = isField && field.FieldType.Name.IndexOf("List") != -1;

            if (isList)
            {
                dynamic list = field.GetValue(this);

                for (int i = 0; i < list.Count; i++)
                {
                    int index = i;
                    if (isField)
                    {
                        getVal = () => list[index];
                        node.AddUi(SetUI(member, ui, styles, field.Name + "_" + i, getVal));
                    }
                    else
                    {
                        node.AddUi(SetUI(member, ui, styles, "", null));
                    }
                }
            }
            else
            {
                if (isField)
                {
                    getVal = () =>
                    {
                        object res = field.GetValue(this);
                        if (res != null)
                        {
                            return res;
                        }
                        return null;
                    };
                    node.AddUi(SetUI(member, ui, styles, field.Name, getVal));
                }
                else
                {
                    node.AddUi(SetUI(member, ui, styles, "", null));
                }
            }


            for (int i = 0; i < back; i++)
            {
                node = node.Parent;
            }
        }
    }

    private void OnGUI()
    {
        //GUIStyle panel = new GUIStyle();
        //panel.margin = new RectOffset(0, 0, 0, 0);
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUIStyle.none, GUIStyle.none);
        Render(_root);
        EditorGUILayout.EndScrollView();
    }

    private void Render(LayoutNode node)
    {
        if (node.layout != null)
        {
            node.layout();
        }
        else
        {
            NormalRender(node)();
        }

    }


    /// <summary>
    /// 设置样式绘制函数
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="styles"></param>
    /// <returns></returns>
    private Func<GUIStyle> SetStyle(object ui, List<object> styles = null)
    {
        if (ui == null) return null;

        GUIStyle Style()
        {
            GUIStyle guiStyle = GenerateStyle(ui);

            if (styles != null)
            {
                //创建GUIStyle
                foreach (var style in styles)
                {
                    //switch (style)
                    //{

                    //}
                }
            }

            return guiStyle;
        }

        return Style;
    }

    /// <summary>
    /// 设置GUILayout
    /// </summary>
    /// <param name="ui"></param>
    /// <param name="styles"></param>
    /// <returns></returns>
    private Func<GUILayoutOption[]> SetOption(object ui, List<object> styles = null)
    {
        if (ui == null) return null;

        GUILayoutOption[] Action()
        {
            List<GUILayoutOption> options = new List<GUILayoutOption>();
            if (styles != null)
            {
                //创建GUILayoutOption
                foreach (var style in styles)
                {
                    switch (style)
                    {
                        case ES_Size size:
                            ESPercent percent = size.GetSizeType();
                            Vector2 vec2Size = size.GetSize();

                            vec2Size = LayoutGenerator.CalSize(percent, vec2Size, this);

                            options.Add(GUILayout.Width(vec2Size.x));
                            options.Add(GUILayout.Height(vec2Size.y));

                            break;
                    }
                }
            }

            return options.ToArray();
        }

        return Action;
    }

    /// <summary>
    /// 设置布局
    /// </summary>
    /// <param name="layout"></param>
    /// <param name="node"></param>
    /// <param name="member"></param>
    /// <returns></returns>
    private Action<GUIStyle, GUILayoutOption[]> SetLayout(object layout, LayoutNode node, MemberInfo member)
    {
        if (layout == null) return null;
        switch (layout)
        {
            case EL_Horizontal:
                return LayoutGenerator.GenerateHorizontal(NormalRender(node));
            case EL_Vertical:
                return LayoutGenerator.GenerateVertical(NormalRender(node));
            case EL_List elList:
                Action render = elList.ListType() == EL_ListType.Flex ? FlexRender(node, member) : NormalRender(node);
                return LayoutGenerator.GenerateList(render, elList, this);
            case EL_Foldout foldout:
                return LayoutGenerator.GenerateFoldout(NormalRender(node), foldout);
        }
        return null;
    }

    /// <summary>
    /// 设置UI
    /// </summary>
    /// <param name="member"></param>
    /// <param name="ui"></param>
    /// <param name="styles"></param>
    /// <param name="propName"></param>
    /// <param name="getVal"></param>
    /// <returns></returns>
    private UIListData SetUI(MemberInfo member, object ui, List<object> styles, string propName, Func<object> getVal)
    {
        UIListData list = new UIListData();
        //创建ui
        if (member.MemberType == MemberTypes.Field)
        {
            //字段
            Action<GUIStyle, GUILayoutOption[]> action = null;

            object val = getVal();

            switch (ui)
            {
                case E_Label:

                    string LabelName()
                    {
                        return (string)getVal();
                    }

                    action = UIGenerator.GenerateLabel(LabelName);
                    break;
                case E_Input input:
                    action = UIGenerator.GenerateInput(propName, (string)val,
                        this, input.GetWidth(), input.IsPercent(), input.IsDoubleLine());
                    break;
                case E_Texture:
                    action = UIGenerator.GenerateObject(propName, (Texture)val);
                    break;
            }

            list.style = SetStyle(ui, styles);
            list.options = SetOption(ui, styles);
            list.action = action;
        }
        else if (member.MemberType == MemberTypes.Method)
        {
            //方法
            Action<GUIStyle, GUILayoutOption[]> action = null;
            switch (ui)
            {
                case E_Button attr:
                    action = UIGenerator.GenerateButton(attr.GetName(), (MethodInfo)member, this);
                    break;
            }

            list.style = SetStyle(ui, styles);
            list.options = SetOption(ui, styles);
            list.action = action;
        }

        return list;
    }

    /// <summary>
    /// 根据GUIStyle创建一个新的GUIStyle
    /// </summary>
    /// <param name="ui"></param>
    /// <returns></returns>
    private GUIStyle GenerateStyle(object ui)
    {
        GUIStyle layoutDef;
        switch (ui)
        {
            case E_Label:
                layoutDef = new GUIStyle(GUI.skin.label);
                return layoutDef;
            case E_Input:
                layoutDef = new GUIStyle(GUI.skin.textField)
                {
                    wordWrap = true
                };
                return layoutDef;
            case E_Button:
                layoutDef = new GUIStyle(GUI.skin.button);
                return layoutDef;
            case EL_Horizontal:
            case EL_Vertical:
            case EL_List:
                layoutDef = new GUIStyle(GUI.skin.box);
                return layoutDef;
        }
        return new GUIStyle();
    }

    /// <summary>
    /// 普通渲染
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    private Action NormalRender(LayoutNode node)
    {
        void Action()
        {
            foreach (var ui in node.list)
            {
                if (ui is LayoutNode layoutNode)
                {
                    Render(layoutNode);
                }
                else
                {
                    UIListData data = (UIListData)ui;
                    data.action(data.style(), data.options());
                }
            }
        }

        return Action;
    }

    /// <summary>
    /// 流式渲染
    /// </summary>
    /// <param name="node"></param>
    /// <param name="member"></param>
    /// <returns></returns>
    private Action FlexRender(LayoutNode node, MemberInfo member)
    {
        void Action()
        {
            int i = 0;
            foreach (var ui in node.list)
            {
                int index = i;

                float space = 6;
                //由于难以正确获取列表对象大小，因此请手动传入
                ES_Size size = member.GetAttribute<ES_Size>();
                ESPercent percent = size.GetSizeType();
                Vector2 vec2Size = size.GetSize();
                vec2Size = LayoutGenerator.CalSize(percent, vec2Size, this);

                if (index == 0)
                {
                    _curWidth = space;
                }

                _curWidth += vec2Size.x + space;

                _totalWidth = position.width;

                if (index < node.list.Count && _curWidth > _totalWidth)
                {
                    _curWidth = vec2Size.x + space * 2;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal(new GUIStyle(GUI.skin.box));
                }

                if (ui is LayoutNode layoutNode)
                {
                    Render(layoutNode);
                }
                else
                {
                    UIListData data = (UIListData)ui;
                    data.action(data.style(), data.options());
                }

                i++;
            }
        }

        return Action;
    }
}
