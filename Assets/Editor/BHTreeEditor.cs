using UnityEditor;

namespace GraphViewExtension
{
    [EName("行为树工具")]
    public class BHTreeEditor : BaseEditor<BHTreeEditor>
    {
        private GGraph _view;

        [MenuItem("GraphView/Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<BHTreeEditor>();
            window.Show();
            window.InitGraph();
        }

        public void InitGraph()
        {
            var provider = CreateInstance<GSearchWindow>();
            var graph = new GGraph(this, provider);
            rootVisualElement.Add(graph);
            _view = graph;
        }
    }
}