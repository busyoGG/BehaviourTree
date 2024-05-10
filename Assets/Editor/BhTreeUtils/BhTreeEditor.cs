using System.Collections.Generic;
using BhTree;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;

namespace BhTreeUtils
{
    [EName("行为树工具")]
    public class BhTreeEditor: BaseEditor<BhTreeEditor>
    {
        private BhTreeGraph _view;
        [MenuItem("BehaviourTree/Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<BhTreeEditor>();
            window.Show();
            window.InitGraph();
            window.SetupToolbar();
        }

        public void InitGraph()
        {
            var provider = CreateInstance<BhSearchWindow>();
            var graph = new BhTreeGraph(this,provider);
            rootVisualElement.Add(graph);
            _view = graph;
        }
        
        private void SetupToolbar()
        {
            var toolbar = new Toolbar();
            var openBtn = new ToolbarButton { text = "打开" };
            openBtn.clicked += Open;
            var saveBtn = new ToolbarButton { text = "保存" };
            saveBtn.clicked += Save;
            toolbar.Add(openBtn);
            toolbar.Add(saveBtn);
            _view.Add(toolbar);
        }

        private void Open()
        {
            
        }

        private void Save()
        {
            // Debug.Log("保存");
            // BhTreeSO so = CreateInstance<BhTreeSO>();
            //
            // string filePath = EditorUtility.SaveFilePanel("保存到本地", Application.dataPath, "NewFile", "asset");
            //
            // if (filePath != "")
            // {
            //     filePath = "Assets" + filePath.Replace(Application.dataPath, "");
            //
            //     AssetDatabase.CreateAsset(so, filePath); // 创建Asset文件
            //     AssetDatabase.SaveAssets(); // 保存Asset文件
            //     AssetDatabase.Refresh(); // 刷新Asset文件
            //
            //     // EditorUtility.DisplayDialog("恭喜!","保存成功,路径为: " + filePath,"OK");
            //     ShowNotification(new GUIContent("保存成功,路径为: " + filePath));
            // }

            List<GDataNode> list =  _view.SaveData();
            Debug.Log("保存成功");
        }
    }
}