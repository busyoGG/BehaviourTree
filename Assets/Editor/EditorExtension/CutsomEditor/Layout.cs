using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[EName("UI布局")]
public class Layout : BaseEditor<Layout>
{
    [MenuItem("Test/Layout")]
    public static void ShowWindow()
    {
        GetWindow<Layout>().Show();
    }

    [E_Texture, ES_Size(70, 70), EL_Horizontal(true)]
    public Texture horizontal1;
    [E_Texture, ES_Size(70, 70)]
    public Texture horizontal2;
    [E_Texture, ES_Size(70, 70), EL_Horizontal(false)]
    public Texture horizontal3;

    [E_Texture, ES_Size(70, 70), EL_Vertical(true)]
    public Texture vertical1;
    [E_Texture, ES_Size(70, 70)]
    public Texture vertical2;
    [E_Texture, ES_Size(70, 70), EL_Vertical(false)]
    public Texture vertical3;

    [E_Texture, ES_Size(70, 70), EL_Foldout(true, "折叠")]
    public Texture foldout1;
    [E_Texture, ES_Size(70, 70), EL_Foldout(false)]
    public Texture foldout2;


    [E_Texture, ES_Size(70, 70), EL_List(true, EL_ListType.Vertical, false)]
    public Texture listV1;
    [E_Texture, ES_Size(70, 70)]
    public Texture listV2;
    [E_Texture, ES_Size(70, 70), EL_List(false)]
    public Texture listV3;

    [E_Texture, ES_Size(70, 70), EL_List(true, EL_ListType.Horizontal, false)]
    public Texture listH1;
    [E_Texture, ES_Size(70, 70)]
    public Texture listH2;
    [E_Texture, ES_Size(70, 70), EL_List(false)]
    public Texture listH3;

    [E_Texture, ES_Size(70, 70), EL_List(true, EL_ListType.Flex, false)]
    public Texture listF1;
    [E_Texture, ES_Size(70, 70)]
    public Texture listF2;
    [E_Texture, ES_Size(70, 70), EL_List(false)]
    public Texture listF3;

    [E_Texture, ES_Size(70, 70), EL_List(true, EL_ListType.Vertical, false, true, 100, 100, ESPercent.Width)]
    public Texture listS1;
    [E_Texture, ES_Size(70, 70)]
    public Texture listS2;
    [E_Texture, ES_Size(70, 70), EL_List(false)]
    public Texture listS3;

    [E_Texture, ES_Size(70, 70), EL_List(true, EL_ListType.Vertical, true, true, 100, 100, ESPercent.Width)]
    public List<Texture> singleList = new List<Texture>() { null, null, null };
}
