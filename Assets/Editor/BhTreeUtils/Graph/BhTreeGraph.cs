using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BhTree
{
    public class BhTreeGraph: GraphView
    {
        private EditorWindow _editorWindow;

        private BhSearchWindow _seachWindow;
        
        private Vector2 _defaultNodeSize = new Vector2(166,110);
        

        public BhTreeGraph(EditorWindow editorWindow, BhSearchWindow provider)
        {
            _editorWindow = editorWindow;
            _seachWindow = provider;
            
            //在右键菜单中添加节点
            provider.onSelectEntryHandler = (searchTreeEntry, searchWindowContext) =>
            {
                var windowRoot = _editorWindow.rootVisualElement;
                var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot, searchWindowContext.screenMousePosition - _editorWindow.position.position);
                var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
                var type = searchTreeEntry.userData as Type;
                CreateNode(type, graphMousePosition);
                return true;
            };
            
            nodeCreationRequest += context =>
            {
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), provider);
                // AddElement(new RootNode());
            };
            
            //尺寸和父控件相同
            this.StretchToParentSize();
            //滚轮缩放
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            //窗口内容拖动
            this.AddManipulator(new ContentDragger());
            //选中Node移动功能
            this.AddManipulator(new SelectionDragger());
            //多个node框选功能
            this.AddManipulator(new RectangleSelector());
            
            //加载样式表和网格
            var styleSheet = EditorGUIUtility.Load("Assets/Editor/BhTreeUtils/Graph/GridBackground.uss") as StyleSheet;
            styleSheets.Add(styleSheet);
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            //存储符合条件的兼容的端口
            List<Port> compatiblePorts = new List<Port>();
            //遍历Graphview中所有的Port 从中寻找
            ports.ForEach(
                (port) =>
                {
                    if (startPort.node != port.node && startPort.direction != port.direction)
                    {
                        compatiblePorts.Add(port);
                    }
                }
            );
            return compatiblePorts;
        }
        
        public RootNode CreateNode(Type type,Vector2 position)
        {
            RootNode node = Activator.CreateInstance(type) as RootNode;
            //这里只用到了position
            node.SetPosition(new Rect(position, Vector2.zero));
            node.RegistResize(this);
            node.SetSize(_defaultNodeSize);
            AddElement(node);
            return node;
        }

        
        public Edge MakeEdge(Port oput, Port iput)
        {
            var edge = new Edge { output = oput, input = iput };
            edge?.input.Connect(edge);
            edge?.output.Connect(edge);
            AddElement(edge);
            return edge;
        }
    }
}