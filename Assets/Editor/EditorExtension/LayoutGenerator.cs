
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class LayoutGenerator
{
    /// <summary>
    /// 生成水平布局
    /// </summary>
    /// <param name="isStart"></param>
    /// <returns></returns>
    public static Action<GUIStyle, GUILayoutOption[]> GenerateHorizontal(Action renderAction)
    {
        Action<GUIStyle, GUILayoutOption[]> action = (GUIStyle style, GUILayoutOption[] options) =>
        {
            EditorGUILayout.BeginHorizontal(style, options);
            renderAction();
            GUILayout.EndHorizontal();
        };
        return action;
    }

    /// <summary>
    /// 生成竖直布局
    /// </summary>
    /// <param name="isStart"></param>
    /// <returns></returns>
    public static Action<GUIStyle, GUILayoutOption[]> GenerateVertical(Action renderAction)
    {
        Action<GUIStyle, GUILayoutOption[]> action = (GUIStyle style, GUILayoutOption[] options) =>
        {
            EditorGUILayout.BeginVertical(style, options);
            renderAction();
            GUILayout.EndVertical();
        };
        return action;
    }

    /// <summary>
    /// 生成折叠
    /// </summary>
    /// <param name="isStart"></param>
    /// <returns></returns>
    public static Action<GUIStyle, GUILayoutOption[]> GenerateFoldout(Action renderAction, EL_Foldout elFoldout)
    {
        Action<GUIStyle, GUILayoutOption[]> action = (GUIStyle style, GUILayoutOption[] options) =>
        {
            elFoldout.IsOpen(EditorGUILayout.BeginFoldoutHeaderGroup(elFoldout.IsOpen(), elFoldout.Name()));
            if (elFoldout.IsOpen())
            {
                renderAction();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
        };
        return action;
    }


    /// <summary>
    /// 生成列表
    /// </summary>
    /// <param name="isStart"></param>
    /// <param name="elList"></param>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static Action<GUIStyle, GUILayoutOption[]> GenerateList(Action renderAction,EL_List elList, EditorWindow obj)
    {
        Action< GUIStyle, GUILayoutOption[]> action = (GUIStyle style, GUILayoutOption[] options) =>
        {
            Vector2 size = CalSize(elList.GetPercent(), new Vector2(elList.Width(), elList.Height()), obj);
            switch (elList.ListType())
            {
                case EL_ListType.Vertical:
                    if (elList.Scroll())
                    {
                        elList.ScrollPosition(EditorGUILayout.BeginScrollView(elList.ScrollPosition(), style, GUILayout.Height(size.y), GUILayout.Width(size.x)));
                        EditorGUILayout.BeginVertical();

                        renderAction();

                        EditorGUILayout.EndVertical();
                        EditorGUILayout.EndScrollView();
                    }
                    else
                    {
                        EditorGUILayout.BeginVertical(style, GUILayout.Height(size.y), GUILayout.Width(size.x));
                        renderAction();
                        EditorGUILayout.EndVertical();
                    }
                    break;
                case EL_ListType.Horizontal:
                case EL_ListType.Flex:
                    if (elList.Scroll())
                    {
                        elList.ScrollPosition(EditorGUILayout.BeginScrollView(elList.ScrollPosition(), style, GUILayout.Height(size.y), GUILayout.Width(size.x)));
                        EditorGUILayout.BeginHorizontal();

                        renderAction();

                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndScrollView();
                    }
                    else
                    {
                        EditorGUILayout.BeginHorizontal(style, GUILayout.Height(size.y), GUILayout.Width(size.x));
                        renderAction();
                        EditorGUILayout.EndHorizontal();
                    }
                    break;
            }
        };
        return action;
    }

    /// <summary>
    /// 计算Size
    /// </summary>
    /// <param name="percent"></param>
    /// <param name="vec2Size"></param>
    /// <returns></returns>
    public static Vector2 CalSize(ESPercent percent, Vector2 vec2Size, EditorWindow obj)
    {
        Vector2 size = obj.position.size - new Vector2(6, 6);
        switch (percent)
        {
            case ESPercent.All:
                vec2Size = size * vec2Size / 100;
                break;
            case ESPercent.Width:
                vec2Size.x = size.x * vec2Size.x / 100;
                break;
            case ESPercent.Height:
                vec2Size.y = size.y * vec2Size.y / 100;
                break;
        }
        return vec2Size;
    }
}
