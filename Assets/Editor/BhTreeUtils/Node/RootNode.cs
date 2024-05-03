using System;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BhTree
{
    public delegate void SetFieldDelegate(object target, object value);
    public class RootNode : Node
    {
        private BhTreeGraph _graph;
        Port _inputPort;
        Port _outputPort;

        private readonly BindingFlags _flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                              BindingFlags.Instance | BindingFlags.DeclaredOnly;

        private Type _type;
        
        private Vector2 _mouseOffset;

        private bool _isResizing = false;

        // [GraphNode(NodeTypeEnum.Input,"��������")] private string _test = "";

        public RootNode()
        {
            //�������˿�
            _inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Single, typeof(Node));
            _inputPort.portName = "Input";
            inputContainer.Add(_inputPort);
            //�������˿�
            _outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Node));
            _outputPort.portName = "Output";
            outputContainer.Add(_outputPort);
            Init();
            
            RegisterCallback<MouseDownEvent>(ResizeStart);
            
            //ˢ�� ��Ȼ������ʾBUG
            RefreshExpandedState();
            RefreshPorts();
        }

        public void SetSize(Vector2 size)
        {
            style.width = size.x;
            style.height = size.y;
        }

        public void RegistResize(BhTreeGraph graph)
        {
            _graph = graph;
            _graph.RegisterCallback<MouseMoveEvent>(ResizeMove);
            _graph.RegisterCallback<MouseUpEvent>(ResizeEnd);
        }

        private void ResizeStart(MouseDownEvent evt)
        {
            if (evt.button == 0 && IsResizeArea(evt.localMousePosition))
            {
                _mouseOffset = evt.mousePosition - layout.size;
                _isResizing = true;
                evt.StopPropagation();
            }
        }
        
        private void ResizeMove(MouseMoveEvent evt)
        {
            if (_isResizing)
            {
                Vector2 newSize = evt.mousePosition - _mouseOffset;
                newSize = Vector2.Max(new Vector2(166,110), newSize);
                // ���½ڵ�Ĵ�С
                style.width = newSize.x;
                style.height = newSize.y;
                evt.StopPropagation();
            }
        }
        
        private void ResizeEnd(MouseUpEvent evt)
        {
            if (_isResizing)
            {
                Debug.Log(layout.size);
                _isResizing = false;
                evt.StopPropagation();
            }
        }
        
        private bool IsResizeArea(Vector2 position)
        {
            // ������Ҫ����resizer�������������½�Ϊ��
            return position.x >= layout.width - 10f &&
                   position.y >= layout.height - 10f;
        }

        private void Init()
        {
            InitConfig();
            //�������
            _type = GetType();

            FieldInfo[] fields = _type.GetFields(_flag);

            mainContainer.style.backgroundColor = new Color(0.5f,0.5f,0.5f,1f);
            mainContainer.style.color = Color.black;
            mainContainer.style.flexGrow = 1;

            foreach (var field in fields)
            {
                GraphNode attr = field.GetCustomAttribute<GraphNode>();

                if (attr != null)
                {
                    object value = field.GetValue(this);

                    VisualElement ele;
                    SetFieldDelegate setValue = (SetFieldDelegate)Delegate.CreateDelegate(typeof(SetFieldDelegate), field, "SetValue", false);
                    switch (attr.Type())
                    {
                        case NodeTypeEnum.Label:
                            Label label = new Label();
                            label.style.fontSize = 20;
                            label.text = value.ToString();
                            mainContainer.Add(label);
                            break;
                        case NodeTypeEnum.Input:
                            ele = new VisualElement();
                            ele.style.flexDirection = FlexDirection.Row;

                            string[] extra = attr.GetExtra();
                            
                            Label inputLabel = new Label();
                            inputLabel.style.fontSize = 20;
                            inputLabel.text = extra.Length > 0 ? extra[0] : field.Name;
                            ele.Add(inputLabel);
                            
                            TextField text = new TextField();
                            text.style.flexGrow = 1;
                            text.style.fontSize = 20;
                            
                            text.RegisterValueChangedCallback(evt =>
                            {
                                setValue(this,evt.newValue);
                            });
                            ele.Add(text);
                            mainContainer.Add(ele);
                            break;
                    }
                }
                
                // AdjustContainerSize();
            }
        }

        protected virtual void InitConfig()
        {
            title = "Ĭ�Ͻڵ�";
        }
        
        // private void AdjustContainerSize()
        // {
        //     // ��ȡ���ݵĸ߶�
        //     float contentHeight = mainContainer.Children().Sum(child => child.layout.height);
        //     // �����������ĸ߶�Ϊ���ݵĸ߶�
        //     mainContainer.style.height = contentHeight;
        // }
    }
}