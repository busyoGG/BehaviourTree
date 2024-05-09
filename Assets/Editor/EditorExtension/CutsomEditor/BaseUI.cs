using UnityEditor;
using UnityEngine;

[EName("基础UI")]
public class BaseUI : BaseEditor<BaseUI>
{
    [MenuItem("Test/BaseUI")]
    public static void ShowWindow()
    {
        GetWindow<BaseUI>().Show();
    }

    [E_Label]
    public string label = "Label样式";

    [E_Input]
    public string defInput;

    [E_Input(50, false)]
    public string strInput;

    [E_Texture]
    public Texture texture;

    [E_Button("按钮")]
    public void Button()
    {

    }
}
