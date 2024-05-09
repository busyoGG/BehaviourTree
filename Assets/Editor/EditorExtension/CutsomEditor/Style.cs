using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UI;
using UnityEngine;

[EName("UI样式")]
public class Style : BaseEditor<Style>
{
    [MenuItem("Test/Style")]
    public static void ShowWindow()
    {
        GetWindow<Style>().Show();
    }

    [E_Texture, ES_Size(70, 70)]
    public Texture size1;

    [E_Texture, ES_Size(70, 10, ESPercent.Height)]
    public Texture size2;

    [E_Texture, ES_Size(50, 70, ESPercent.Width)]
    public Texture size3;

    [E_Texture, ES_Size(50, 10, ESPercent.All)]
    public Texture size4;
}
