using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using UnityEditor;
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

        public string guid;

        /// <summary>
        /// 是否完成渲染初始化
        /// </summary>
        private bool _inited = false;

        /// <summary>
        /// 当前类类型
        /// </summary>
        private Type _type;

        /// <summary>
        /// 默认尺寸
        /// </summary>
        private Vector2 _defSize;

        /// <summary>
        /// 当前尺寸
        /// </summary>
        private Vector2 _curSize;

        /// <summary>
        /// 默认位置
        /// </summary>
        private Vector2 _defPos;

        /// <summary>
        /// 默认高度
        /// </summary>
        private float _defHeight;

        /// <summary>
        /// 默认宽度
        /// </summary>
        private float _defWidth;

        /// <summary>
        /// 鼠标按下时坐标
        /// </summary>
        private Vector2 _mouseOffset;

        /// <summary>
        /// 是否正在调整大小
        /// </summary>
        private bool _isResizing = false;

        /// <summary>
        /// 是否已描边
        /// </summary>
        private bool _isBordered = false;

        /// <summary>
        /// 默认字体尺寸
        /// </summary>
        private int _fontSize = 16;

        /// <summary>
        /// 描边补正
        /// </summary>
        private float _borderOffset = 0;

        /// <summary>
        /// 是否执行到当前节点
        /// </summary>
        private bool _isRun = false;

        /// <summary>
        /// 是否选中当前节点
        /// </summary>
        private bool _isSelected = false;

        //----- 描边颜色 ------ start

        protected Color _selectColor = Color.red;

        protected Color _parentColor = Color.green;

        protected Color _runColor = Color.green;

        protected Color _passColor = Color.yellow;

        //----- 描边颜色 ------ end

        /// <summary>
        /// 节点类型
        /// </summary>
        protected string _NodeType = "None";

        /// <summary>
        /// 节点数据
        /// </summary>
        protected dynamic _data = new ExpandoObject();
        
        /// <summary>
        /// 数据节点
        /// </summary>
        GDataNode _dataNode = new GDataNode();

        public RootNode()
        {
            //生成唯一标识
            guid = GUID.Generate().ToString();
            Label lbGuid = new Label(guid);
            lbGuid.style.position = Position.Absolute;
            lbGuid.style.top = -20;
            lbGuid.style.alignSelf = Align.Center;
            Add(lbGuid);

            //添加输入端口
            _inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Node));
            _inputPort.portName = "Parent";
            inputContainer.Add(_inputPort);
            //添加输出端口
            _outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Node));
            _outputPort.portName = "Child";
            outputContainer.Add(_outputPort);

            RegisterCallback<MouseDownEvent>(ResizeStart);
            RegisterCallback<GeometryChangedEvent>(OnEnable);
        }

        private void OnEnable(GeometryChangedEvent evt)
        {
            Debug.Log("创建完成，开始渲染  " + layout.size);
            _defHeight = titleContainer.layout.height + _outputPort.layout.height;
            _defWidth = titleContainer.layout.width;
            _defPos = layout.position;

            Init();
            schedule.Execute(() =>
            {
                UpdateNodeSize();
                _inited = true;
            }).StartingIn(1);
            //刷新 不然会有显示BUG
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
        /// 设置运行状态，请在执行到该节点时调用
        /// </summary>
        /// <param name="run"></param>
        public void SetRun(bool run)
        {
            if (run)
            {
                _isRun = true;
                SetBorder(_runColor, 5);
            }
            else
            {
                if (_isRun)
                {
                    SetBorder(_passColor, 5);
                }
                else
                {
                    SetBorder(Color.white, 0);
                }

                _isRun = false;
            }
        }

        /// <summary>
        /// 清除所有运行状态
        /// </summary>
        public void ClearRunState()
        {
            //执行两次确保完全清除
            SetRun(false);
            SetRun(false);
            //遍历节点
            var connections = _outputPort.connections;
            foreach (var edge in connections)
            {
                RootNode node = edge.input.node as RootNode;
                node?.ClearRunState();
            }
        }

        public void Selected(RootNode leaf = null)
        {
            if (leaf == null)
            {
                SetBorder(_selectColor, 5);
            }
            else
            {
                SetBorder(_parentColor, 5);
            }

            //遍历节点
            var connections = _inputPort.connections;
            foreach (var edge in connections)
            {
                RootNode node = edge.output.node as RootNode;
                node?.Selected(node);
            }
        }

        public void UnSelected()
        {
            SetBorder(_selectColor, 0);
            //遍历节点
            var connections = _inputPort.connections;
            foreach (var edge in connections)
            {
                RootNode node = edge.output.node as RootNode;
                node?.UnSelected();
            }
        }

        /// <summary>
        /// 判断是否有前置节点
        /// </summary>
        /// <returns></returns>
        public bool HasParent()
        {
            return _inputPort.connections.Any();
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        /// <returns></returns>
        public GDataNode SaveData()
        {
            _data.type = _NodeType;
            SetData();
            _dataNode.SetData(_data);
            //遍历节点
            var connections = _outputPort.connections;
            foreach (var edge in connections)
            {
                RootNode node = edge.input.node as RootNode;
                _dataNode.AddChild(node.SaveData());
            }
            return _dataNode;
        }
        
        /// <summary>
        /// 设置描边颜色，最好是在所有初始化完成后再调用
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
                UpdateBorderSize();
            }
        }

        private void ResizeStart(MouseDownEvent evt)
        {
            if (evt.button == 0)
            {
                //判断改大小
                if (IsResizeArea(evt.localMousePosition))
                {
                    _mouseOffset = evt.mousePosition - layout.size;
                    _isResizing = true;
                    evt.StopPropagation();
                }
            }
        }

        private void ResizeMove(MouseMoveEvent evt)
        {
            if (_isResizing)
            {
                Vector2 newSize = evt.mousePosition - _mouseOffset;
                newSize = Vector2.Max(_defSize, newSize);
                // 更新节点的大小
                UpdateSize(newSize);

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

            _defPos = layout.position + new Vector2(_borderOffset * 0.5f, _borderOffset * 0.5f);
        }

        private bool IsResizeArea(Vector2 position)
        {
            // 根据需要定义resizer区域，这里以右下角为例
            return position.x >= layout.width - 10f - _borderOffset &&
                   position.y >= layout.height - 10f - _borderOffset;
        }

        private void Init()
        {
            InitConfig();
            //获得类型
            _type = GetType();

            FieldInfo[] fields = _type.GetFields(_flag);

            //设置标题颜色
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

            int boxCount = -1;
            var box = new VisualElement();
            extensionContainer.Add(box);

            foreach (var field in fields)
            {
                GraphNode attr = field.GetCustomAttribute<GraphNode>();

                if (attr != null)
                {
                    //辅助名称
                    GName gName = field.GetCustomAttribute<GName>();
                    string fieldName = field.Name.Replace("_", "");
                    string subName = gName != null ? gName.GetName() : fieldName.Substring(0,1).ToUpper() + fieldName.Substring(1);
                    
                    //容器宽度
                    GWidth gWidth = field.GetCustomAttribute<GWidth>();
                    Length len = gWidth != null ? gWidth.GetLength() : new Length(96, LengthUnit.Percent);
                    
                    if (boxCount == 0)
                    {
                        box = new VisualElement();
                        extensionContainer.Add(box);
                    }

                    if (boxCount > -1)
                    {
                        boxCount--;
                    }

                    object value = field.GetValue(this);

                    VisualElement ele = new VisualElement();
                    ele.style.width = len;
                    ele.style.marginTop = 8;
                    ele.style.alignSelf = Align.Center;

                    Label foreLabel;

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

                            foreLabel = new Label();
                            foreLabel.style.fontSize = _fontSize;
                            foreLabel.text = subName;
                            ele.Add(foreLabel);

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
                        case NodeTypeEnum.Enum:
                            ele.style.flexDirection = FlexDirection.Row;

                            foreLabel = new Label();
                            foreLabel.style.fontSize = _fontSize;
                            foreLabel.text = subName;
                            ele.Add(foreLabel);

                            EnumField enumField = new EnumField(value as Enum);
                            enumField.style.flexGrow = 1;

                            enumField.RegisterValueChangedCallback(evt => setValue(this, evt.newValue));

                            ele.Add(enumField);
                            break;

                        case NodeTypeEnum.Slide:
                            ele.style.flexDirection = FlexDirection.Row;

                            foreLabel = new Label();
                            foreLabel.style.fontSize = _fontSize;
                            foreLabel.text = subName;
                            ele.Add(foreLabel);

                            bool isInt = extra[0] == "Int";

                            Label num = new Label();
                            num.style.fontSize = _fontSize;
                            num.style.width = _fontSize * 3;
                            num.style.unityTextAlign = TextAnchor.MiddleRight;

                            if (isInt)
                            {
                                SliderInt slider = new SliderInt(int.Parse(extra[1]), int.Parse(extra[2]));

                                num.text = extra[2];

                                slider.value = 0;
                                slider.style.flexGrow = 1;

                                slider.RegisterValueChangedCallback(evt =>
                                {
                                    setValue(this, evt.newValue);
                                    num.text = evt.newValue.ToString();
                                });

                                ele.Add(slider);
                            }
                            else
                            {
                                Slider slider = new Slider(float.Parse(extra[0]), float.Parse(extra[1]));

                                num.text = extra[1];

                                slider.value = 0;
                                slider.style.flexGrow = 1;

                                slider.RegisterValueChangedCallback(evt =>
                                {
                                    setValue(this, evt.newValue);
                                    num.text = evt.newValue.ToString();
                                });

                                ele.Add(slider);
                            }

                            ele.Add(num);

                            break;
                        case NodeTypeEnum.Radio:

                            RadioButtonGroup radioButtonGroup = new RadioButtonGroup();
                            radioButtonGroup.value = 0;
                            
                            ele.Add(radioButtonGroup);

                            foreach (var selection in extra)
                            {
                                RadioButton radio = new RadioButton();
                                radio.Children().FirstOrDefault().style.flexGrow = 0;

                                Label lbRadio = new Label();
                                lbRadio.style.fontSize = _fontSize;
                                lbRadio.text = selection;
                                radio.Add(lbRadio);

                                radioButtonGroup.Add(radio);
                            }

                            radioButtonGroup.RegisterValueChangedCallback(evt => { setValue(this, evt.newValue); });
                            break;
                        case NodeTypeEnum.Toggle:

                            Toggle toggle = new Toggle();
                            toggle.style.width = StyleKeyword.Auto;
                            toggle.Children().FirstOrDefault().style.flexGrow = 0;

                            toggle.RegisterValueChangedCallback(evt => { setValue(this, evt.newValue); });

                            ele.Add(toggle);

                            foreLabel = new Label();
                            foreLabel.style.fontSize = _fontSize;
                            foreLabel.text = subName;
                            toggle.Add(foreLabel);

                            break;
                        case NodeTypeEnum.Box:

                            if (gName != null)
                            {
                                foreLabel = new Label();
                                foreLabel.style.fontSize = _fontSize * 0.8f;
                                foreLabel.text = subName;
                                ele.Add(foreLabel);
                            }

                            Color color = Color.yellow;
                            GColor gColor = field.GetCustomAttribute<GColor>();

                            if (gColor != null)
                            {
                                color = gColor.GetColor();
                            }

                            box = new VisualElement();
                            box.style.backgroundColor = color;
                            box.style.marginTop = 8;
                            box.style.width = new Length(96, LengthUnit.Percent);
                            box.style.alignSelf = Align.Center;
                            box.style.borderBottomLeftRadius = 5;
                            box.style.borderBottomRightRadius = 5;
                            box.style.borderTopLeftRadius = 5;
                            box.style.borderTopRightRadius = 5;
                            box.style.flexDirection = extra.Length > 1 && extra[1] == "Column"
                                ? FlexDirection.Column
                                : FlexDirection.Row;
                            box.style.flexWrap = Wrap.Wrap;

                            extensionContainer.Add(box);

                            boxCount = int.Parse(extra[0]);

                            break;
                    }

                    box.Add(ele);
                }
            }
        }

        /// <summary>
        /// 刷新尺寸
        /// </summary>
        /// <param name="size"></param>
        private void UpdateSize(Vector2 size)
        {
            style.width = size.x;
            style.height = size.y;
            if (_isBordered)
            {
                _curSize = size - new Vector2(_borderOffset, _borderOffset);
            }
            else
            {
                _curSize = size;
            }
        }

        /// <summary>
        /// 更新节点尺寸
        /// </summary>
        private void UpdateNodeSize()
        {
            //未手动调整过大小才自动更新
            if (_curSize.Equals(Vector2.zero))
            {
                // 计算内容的总高度
                float contentHeight = _defHeight + extensionContainer.layout.height + 10;

                // 设置 Node 的新高度
                SetSize(new Vector2(_defWidth, contentHeight));
                DrawBorder();
            }
        }

        /// <summary>
        /// 更新带描边的节点尺寸
        /// </summary>
        private void UpdateBorderSize()
        {
            if (_borderOffset == 0)
            {
                _isBordered = false;
                DrawBorder();
            }
            else if (!_isBordered)
            {
                _isBordered = true;
                DrawBorder();
            }
        }

        private void DrawBorder()
        {
            if (_isBordered)
            {
                if (_curSize.Equals(Vector2.zero))
                {
                    style.width = _defSize.x + _borderOffset;
                    style.height = _defSize.y + _borderOffset;
                }
                else
                {
                    style.width = _curSize.x + _borderOffset;
                    style.height = _curSize.y + _borderOffset;
                }

                _defPos = new Vector2(style.left.value.value, style.top.value.value);

                style.left = _defPos.x - _borderOffset * 0.5f;
                style.top = _defPos.y - _borderOffset * 0.5f;
            }
            else
            {
                if (_curSize.Equals(Vector2.zero))
                {
                    style.width = _defSize.x;
                    style.height = _defSize.y;
                }
                else
                {
                    style.width = _curSize.x;
                    style.height = _curSize.y;
                }

                style.left = _defPos.x;
                style.top = _defPos.y;
            }
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        protected virtual void InitConfig()
        {
            title = "默认节点";
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        protected virtual void SetData()
        {
        }
    }
}