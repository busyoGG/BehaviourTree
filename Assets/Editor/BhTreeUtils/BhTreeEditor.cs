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
        private GraphView _view;
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
            var toolbarButton = new ToolbarButton { text = "工具" };
            toolbar.Add(toolbarButton);
            _view.Add(toolbar);
        }
    }
}