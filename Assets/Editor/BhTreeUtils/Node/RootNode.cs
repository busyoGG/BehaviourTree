using System;
using System.Linq;
using System.Reflection;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace BhTreeUtils
{
    public delegate void SetFieldDelegate(object target, object value);

    public class RootNode : Node
    {
        private BhTreeGraph _graph;
        Port _inputPort;
        Port _outputPort;

        private readonly BindingFlags _flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                              BindingFlags.Instance | BindingFlags.DeclaredOnly;

        /// <summary>
        /// �Ƿ������Ⱦ��ʼ��
        /// </summary>
        private bool _inited = false;

        /// <summary>
        /// ��ǰ������
        /// </summary>
        private Type _type;

        /// <summary>
        /// Ĭ�ϳߴ�
        /// </summary>
        private Vector2 _defSize;

        /// <summary>
        /// Ĭ�ϸ߶�
        /// </summary>
        private float _defHeight;

        /// <summary>
        /// ��갴��ʱ����
        /// </summary>
        private Vector2 _mouseOffset;

        /// <summary>
        /// �Ƿ����ڵ�����С
        /// </summary>
        private bool _isResizing = false;

        /// <summary>
        /// Ĭ������ߴ�
        /// </summary>
        private int _fontSize = 16;

        /// <summary>
        /// ��߲���
        /// </summary>
        private float _borderOffset = 0;

        /// <summary>
        /// �Ƿ�ִ�е���ǰ�ڵ�
        /// </summary>
        private bool _isRun = false;

        /// <summary>
        /// �ڵ�����
        /// </summary>
        protected string _NodeType = "None";

        public RootNode()
        {
            //�������˿�
            _inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Node));
            _inputPort.portName = "Parent";
            inputContainer.Add(_inputPort);
            //�������˿�
            _outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Node));
            _outputPort.portName = "Child";
            outputContainer.Add(_outputPort);

            RegisterCallback<MouseDownEvent>(ResizeStart);
            RegisterCallback<GeometryChangedEvent>(OnEnable);
        }

        private void OnEnable(GeometryChangedEvent evt)
        {
            Debug.Log("������ɣ���ʼ��Ⱦ  " + layout.size);
            _defHeight = titleContainer.layout.height + _outputPort.layout.height;
            Debug.Log("Ĭ�ϸ߶� " + _defHeight);
            Init();
            schedule.Execute(() =>
            {
                UpdateNodeSize();
                _inited = true;
            }).StartingIn(1);
            //ˢ�� ��Ȼ������ʾBUG
            RefreshExpandedState();
            RefreshPorts();

            UnregisterCallback<GeometryChangedEvent>(OnEnable);
        }

        public void SetSize(Vector2 size)
        {
            style.width = size.x;
            style.height = size.y;
            _defSize = size;
        }

        public void RegistResize(BhTreeGraph graph)
        {
            _graph = graph;
            _graph.RegisterCallback<MouseMoveEvent>(ResizeMove);
            _graph.RegisterCallback<MouseUpEvent>(ResizeEnd);
        }

        /// <summary>
        /// ��������״̬������ִ�е��ýڵ�ʱ����
        /// </summary>
        /// <param name="run"></param>
        public void SetRun(bool run)
        {
            if (run)
            {
                _isRun = true;
                SetBorder(Color.green, 5);
            }
            else
            {
                if (_isRun)
                {
                    SetBorder(Color.yellow, 5);
                }
                else
                {
                    SetBorder(Color.white, 0);
                }

                _isRun = false;
            }
        }

        /// <summary>
        /// �����������״̬
        /// </summary>
        public void ClearRunState()
        {
            //ִ������ȷ����ȫ���
            SetRun(false);
            SetRun(false);
            //�����ڵ�
            var connections = _outputPort.connections;
            foreach (var edge in connections)
            {
                RootNode node = edge.output.node as RootNode;
                node?.ClearRunState();
            }
        }

        /// <summary>
        /// ���������ɫ������������г�ʼ����ɺ��ٵ���
        /// </summary>
        /// <param name="color"></param>
        private void SetBorder(Color color, float width)
        {
            style.borderBottomWidth = width;
            style.borderBottomColor = color;
            style.borderLeftWidth = width;
            style.borderLeftColor = color;
            style.borderRightWidth = width;
            style.borderRightColor = color;
            style.borderTopWidth = width;
            style.borderTopColor = color;
            style.borderBottomLeftRadius = 10;
            style.borderBottomRightRadius = 10;
            style.borderTopLeftRadius = 10;
            style.borderTopRightRadius = 10;

            _borderOffset = width * 2;

            if (_inited)
            {
                UpdateNodeSize();
            }
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
                newSize = Vector2.Max(_defSize, newSize);
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

            //���ñ�����ɫ
            Label lbTitle = mainContainer.Q<Label>("title-label", (string)null);
            lbTitle.style.color = Color.black;
            lbTitle.style.fontSize = 18;
            lbTitle.style.unityFontStyleAndWeight = FontStyle.Bold;

            titleContainer.style.minHeight = 25;
            titleContainer.style.maxHeight = 25;

            mainContainer.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            mainContainer.style.color = Color.black;
            mainContainer.style.flexGrow = 1;

            extensionContainer.style.height = StyleKeyword.Auto;
            // extensionContainer.style.backgroundColor = new Color(0.55f, 1.00f, 0.78f, 0.8f);

            foreach (var field in fields)
            {
                GraphNode attr = field.GetCustomAttribute<GraphNode>();

                if (attr != null)
                {
                    object value = field.GetValue(this);

                    VisualElement ele = new VisualElement();
                    ele.style.width = new Length(96, LengthUnit.Percent);
                    ele.style.marginTop = 8;
                    ele.style.alignSelf = Align.Center;
                    
                    if (attr.Type() != NodeTypeEnum.Note)
                    {
                        ele.style.borderBottomColor = Color.white;
                        ele.style.borderBottomWidth = 2;
                        ele.style.paddingBottom = 3;
                    }

                    SetFieldDelegate setValue =
                        (SetFieldDelegate)Delegate.CreateDelegate(typeof(SetFieldDelegate), field, "SetValue", false);

                    string[] extra = attr.GetExtra();
                    switch (attr.Type())
                    {
                        case NodeTypeEnum.Label:
                            Label label = new Label();
                            label.style.fontSize = _fontSize;
                            label.text = value.ToString();
                            ele.Add(label);
                            break;
                        case NodeTypeEnum.Input:
                            ele.style.flexDirection = FlexDirection.Row;

                            Label inputLabel = new Label();
                            inputLabel.style.fontSize = _fontSize;
                            inputLabel.text = extra.Length > 0 ? extra[0] : field.Name;
                            ele.Add(inputLabel);

                            TextField text = new TextField();
                            text.style.flexGrow = 1;
                            text.style.height = _fontSize * 1.2f;

                            var fontChild = text.Children().FirstOrDefault().Children().FirstOrDefault();
                            fontChild.style.fontSize = _fontSize;

                            text.RegisterValueChangedCallback(evt => { setValue(this, evt.newValue); });
                            ele.Add(text);
                            break;
                        case NodeTypeEnum.Note:
                            ele.style.flexDirection = FlexDirection.Column;
                            ele.style.alignItems = Align.Center;
                            ele.style.justifyContent = Justify.Center;
                            ele.style.backgroundColor = new Color(0.64f, 0.78f, 1f, 1f);
                            ele.style.borderBottomLeftRadius = 5;
                            ele.style.borderBottomRightRadius = 5;
                            ele.style.borderTopLeftRadius = 5;
                            ele.style.borderTopRightRadius = 5;

                            if (extra.Length > 0 && extra[0] == "Custom")
                            {
                                TextField note = new TextField();
                                note.value = value.ToString();
                                note.style.width = ele.style.width;
                                note.style.height = StyleKeyword.Auto;
                                note.multiline = true;
                                note.style.whiteSpace = WhiteSpace.Normal;
                                note.style.height = StyleKeyword.Auto;

                                var child = note.Children().FirstOrDefault();

                                child.style.backgroundColor = Color.clear;
                                child.style.borderBottomColor = Color.clear;
                                child.style.borderTopColor = Color.clear;
                                child.style.borderLeftColor = Color.clear;
                                child.style.borderRightColor = Color.clear;
                                child.style.borderBottomWidth = 0;
                                child.style.borderTopWidth = 0;
                                child.style.borderLeftWidth = 0;
                                child.style.borderRightWidth = 0;
                                child.style.color = Color.black;
                                child.style.unityTextAlign = TextAnchor.MiddleCenter;

                                fontChild = child.Children().FirstOrDefault().Children().FirstOrDefault();
                                fontChild.style.fontSize = _fontSize;


                                // note.style.backgroundImage = null;
                                // note.style.backgroundColor = null;
                                note.RegisterValueChangedCallback((evt) => { setValue(this, evt.newValue); });

                                note.RegisterCallback<FocusOutEvent>(evt => { UpdateNodeSize(); });

                                ele.Add(note);
                            }
                            else
                            {
                                Label note = new Label();
                                note.style.fontSize = _fontSize;
                                note.style.width = StyleKeyword.Auto;
                                note.style.height = StyleKeyword.Auto;
                                note.text = value.ToString();
                                note.style.whiteSpace = WhiteSpace.Normal;

                                ele.Add(note);
                            }

                            break;
                    }

                    extensionContainer.Add(ele);
                }
            }
        }

        private void UpdateNodeSize()
        {
            // �������ݵ��ܸ߶�
            float contentHeight = _defHeight + extensionContainer.layout.height + 10 + _borderOffset;

            // ���� Node ���¸߶�
            SetSize(new Vector2(layout.width + _borderOffset, contentHeight));
        }

        protected virtual void InitConfig()
        {
            title = "Ĭ�Ͻڵ�";
        }
    }
}