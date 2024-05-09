using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[EName("测试窗口")]
public class Test : BaseEditor<Test>
{
    [MenuItem("Test/Test1")]
    public static void ShowWindow()
    {
        GetWindow<Test>().Show();
    }

    [E_Label, EL_Horizontal(true)]
    public string label = "测试Label";

    [E_Input(50, false), ES_Size(20, 40, ESPercent.Width)]
    public string strInput;

    [E_Button("测试按钮"), ES_Size(20, 40, ESPercent.Width), EL_Horizontal(false)]
    public void ShowHello()
    {
        //Debug.Log("点击测试按钮");
        //label = "修改label";
        tex.Add(null);
        Refresh();
    }

    [E_Label, EL_Horizontal(true)]
    public string label2 = "测试Label2";

    [E_Texture, ES_Size(70, 70), EL_Horizontal(false)]
    public Texture texture;

    [E_Texture, ES_Size(70, 70), EL_Foldout(true, "测试折叠"), EL_List(true, EL_ListType.Flex, true, true, 100, 200, ESPercent.Width)]
    public List<Texture> tex = new List<Texture>() { null, null, null, null, null, null };

    [E_Label, ES_Size(70, 70), EL_List(true, EL_ListType.Flex, true), EL_Foldout(false)]
    public List<string> labels = new List<string>() { "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa", "aaaaaaaa" };

    [E_Label, ES_Size(70, 70), EL_List(true, EL_ListType.Vertical, false, true, 100, 200, ESPercent.Width)]
    public string labelL1 = "测凉列表1";
    [E_Label, ES_Size(70, 100)]
    public string labelL2 = "测试列表2";
    [E_Label, ES_Size(70, 70), EL_List(false, EL_ListType.Vertical, false)]
    public string labelL3 = "测试列表3";

    [E_Button("刷新界面")]
    private void Refresh()
    {
        RefreshUIInit();
    }
}
