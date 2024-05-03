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
            
            //���Ҽ��˵�����ӽڵ�
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
            
            //�ߴ�͸��ؼ���ͬ
            this.StretchToParentSize();
            //��������
            SetupZoom(ContentZoomer.DefaultMinScale, ContentZoomer.DefaultMaxScale);
            //���������϶�
            this.AddManipulator(new ContentDragger());
            //ѡ��Node�ƶ�����
            this.AddManipulator(new SelectionDragger());
            //���node��ѡ����
            this.AddManipulator(new RectangleSelector());
            
            //������ʽ�������
            var styleSheet = EditorGUIUtility.Load("Assets/Editor/BhTreeUtils/Graph/GridBackground.uss") as StyleSheet;
            styleSheets.Add(styleSheet);
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();
        }
        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            //�洢���������ļ��ݵĶ˿�
            List<Port> compatiblePorts = new List<Port>();
            //����Graphview�����е�Port ����Ѱ��
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
            //����ֻ�õ���position
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