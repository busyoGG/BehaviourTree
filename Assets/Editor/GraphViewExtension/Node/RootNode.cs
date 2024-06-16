using System;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphViewExtension
{
    public delegate void SetFieldDelegate(object target, object value);

    public class RootNode : Node
    {
        /// <summary>
        /// 所属的GraphView
        /// </summary>
        private GGraph _graph;
        Port _inputPort;
        Port _outputPort;

        /// <summary>
        /// 反射范围标记
        /// </summary>
        private readonly BindingFlags _flag = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static |
                                              BindingFlags.Instance | BindingFlags.DeclaredOnly;

        /// <summary>
        /// 唯一标识符
        /// </summary>
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
        /// 之前的状态是否描边
        /// </summary>
        private bool _lastBordered = false;

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
        /// 节点数据
        /// </summary>
        protected dynamic _data = new ExpandoObject();

        /// <summary>
        /// 是否设置了数据
        /// </summary>
        protected bool _isSet = false;

        /// <summary>
        /// 数据节点
        /// </summary>
        GDataNode _dataNode = new GDataNode();

        public RootNode()
        {
            title = "默认节点";
            _type = GetType();

            //生成唯一标识
            guid = GUID.Generate().ToString();

            //添加输入端口
            _inputPort = InstantiatePort(Orientation.Horizontal, Direction.Input, Port.Capacity.Multi, typeof(Node));
            _inputPort.portName = "Parent";
            inputContainer.Add(_inputPort);
            //添加输出端口
            _outputPort = InstantiatePort(Orientation.Horizontal, Direction.Output, Port.Capacity.Multi, typeof(Node));
            _outputPort.portName = "Child";
            outputContainer.Add(_outputPort);

            //注册鼠标按下监听
            RegisterCallback<MouseDownEvent>(ResizeStart);
            //注册界面改变监听
            RegisterCallback<GeometryChangedEvent>(OnEnable);
        }

        /// <summary>
        /// 界面改变回调
        /// </summary>
        /// <param name="evt"></param>
        private void OnEnable(GeometryChangedEvent evt)
        {
            Debug.Log("创建完成，开始渲染  " + layout.size);
            //保存默认高度
            _defHeight = titleContainer.layout.height + _outputPort.layout.height;
            //保存默认位置
            _defPos = layout.position;

            //如果设置了数据（即通过打开文件创建的节点），则把数据填充到UI
            if (_isSet)
            {
                ResetData();
            }

            //初始化UI
            Init();
            CustomUI();

            //延迟更新节点大小
            schedule.Execute(() =>
            {
                UpdateNodeSize();
                _inited = true;
            }).StartingIn(1);
            
            //刷新 不然会有显示BUG
            RefreshExpandedState();
            RefreshPorts();

            //注销界面改变监听，使该方法只执行一次
            UnregisterCallback<GeometryChangedEvent>(OnEnable);
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="data"></param>
        public void SetData(ExpandoObject data)
        {
            _data = data;
            _isSet = true;
        }

        /// <summary>
        /// 设置尺寸，该方法会保存默认尺寸
        /// </summary>
        /// <param name="size"></param>
        public void SetSize(Vector2 size)
        {
            style.width = size.x;
            style.height = size.y;
            _defSize = size;
        }

        /// <summary>
        /// 在GraphView注册鼠标移动和抬起事件
        /// 用于节点界面尺寸的修改
        /// 在GraphView是为了能够在节点外响应事件
        /// </summary>
        /// <param name="graph"></param>
        public void RegistResize(GGraph graph)
        {
            _graph = graph;
            _graph.RegisterCallback<MouseMoveEvent>(ResizeMove);
            _graph.RegisterCallback<MouseUpEvent>(ResizeEnd);
        }

        /// <summary>
        /// 获取输入端口
        /// </summary>
        /// <returns></returns>
        public Port GetInput()
        {
            return _inputPort;
        }

        /// <summary>
        /// 获取输出端口
        /// </summary>
        /// <returns></returns>
        public Port GetOutput()
        {
            return _outputPort;
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

        /// <summary>
        /// 选中节点描边
        /// </summary>
        /// <param name="leaf"></param>
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

        /// <summary>
        /// 取消选中节点描边
        /// </summary>
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
            //保存基本数据
            _data.guid = guid;
            _data.pos = _defPos.ToString();
            _data.size = (_curSize.Equals(Vector2.zero) ? _defSize : _curSize).ToString();
            _dataNode.SetNodeType(_type.FullName);
            //保存额外数据，用户自定义
            SetData();
            _dataNode.SetData(_data);
            //遍历子节点
            var connections = _outputPort.connections;
            foreach (var edge in connections)
            {
                RootNode node = edge.input.node as RootNode;
                _dataNode.AddChild(node?.SaveData());
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

        /// <summary>
        /// 移除连线
        /// </summary>
        public void RemoveEdges()
        {
            foreach (var edge in _outputPort.connections)
            {
                _graph.RemoveElement(edge);
            }
        }

        /// <summary>
        /// 节点尺寸调节鼠标按下回调
        /// </summary>
        /// <param name="evt"></param>
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

        /// <summary>
        /// 节点尺寸调节鼠标移动回调
        /// </summary>
        /// <param name="evt"></param>
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

        /// <summary>
        /// 节点尺寸调节鼠标抬起回调
        /// </summary>
        /// <param name="evt"></param>
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

        /// <summary>
        /// 是否在节点右下角区域
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private bool IsResizeArea(Vector2 position)
        {
            // 根据需要定义resizer区域，这里以右下角为例
            return position.x >= layout.width - 10f - _borderOffset &&
                   position.y >= layout.height - 10f - _borderOffset;
        }

        /// <summary>
        /// 初始化UI
        /// </summary>
        private void Init()
        {
            InitConfig();

            //添加GUID
            Label lbGuid = new Label(guid);
            lbGuid.style.position = Position.Absolute;
            lbGuid.style.top = -20;
            lbGuid.style.alignSelf = Align.Center;
            Add(lbGuid);
            //获得类型
            _type = GetType();

            FieldInfo[] fields = _type.GetFields(_flag);

            //设置标题颜色
            Label lbTitle = mainContainer.Q<Label>("title-label", (string)null);
            lbTitle.style.color = Color.black;
            lbTitle.style.fontSize = 18;
            lbTitle.style.unityFontStyleAndWeight = FontStyle.Bold;

            //固定titleContainer大小
            titleContainer.style.minHeight = 25;
            titleContainer.style.maxHeight = 25;

            //mainContainer着色
            mainContainer.style.backgroundColor = new Color(0.5f, 0.5f, 0.5f, 1f);
            mainContainer.style.color = Color.black;
            mainContainer.style.flexGrow = 1;

            //extensionContainer自动高度
            extensionContainer.style.height = StyleKeyword.Auto;
            // extensionContainer.style.backgroundColor = new Color(0.55f, 1.00f, 0.78f, 0.8f);

            //ui box 分割。没有box标签的情况下都在默认的第一个box中添加ui
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
                    string subName = gName != null
                        ? gName.GetName()
                        : fieldName.Substring(0, 1).ToUpper() + fieldName.Substring(1);

                    //容器宽度
                    GWidth gWidth = field.GetCustomAttribute<GWidth>();
                    Length len = gWidth != null ? gWidth.GetLength() : new Length(96, LengthUnit.Percent);

                    //判断是否需要增加ui box
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

                    //添加当前UI类型的父容器
                    VisualElement ele = new VisualElement();
                    ele.style.width = len;
                    ele.style.marginTop = 8;
                    ele.style.alignSelf = Align.Center;

                    //辅助标签
                    Label foreLabel;

                    //非注释UI情况下，UI父容器底部描白边
                    if (attr.Type() != NodeTypeEnum.Note)
                    {
                        ele.style.borderBottomColor = Color.white;
                        ele.style.borderBottomWidth = 2;
                        ele.style.paddingBottom = 3;
                    }

                    //设置值的委托函数
                    SetFieldDelegate setValue =
                        (SetFieldDelegate)Delegate.CreateDelegate(typeof(SetFieldDelegate), field, "SetValue", false);

                    string[] extra = attr.GetExtra();
                    //创建UI
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
                            //创建注释背景
                            ele.style.flexDirection = FlexDirection.Column;
                            ele.style.alignItems = Align.Center;
                            ele.style.justifyContent = Justify.Center;
                            ele.style.backgroundColor = new Color(0.64f, 0.78f, 1f, 1f);
                            ele.style.borderBottomLeftRadius = 5;
                            ele.style.borderBottomRightRadius = 5;
                            ele.style.borderTopLeftRadius = 5;
                            ele.style.borderTopRightRadius = 5;

                            //固定注释和可修改注释的情况
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
                            else
                            {
                                Slider slider = new Slider(float.Parse(extra[0]), float.Parse(extra[1]));

                                num.text = extra[0];

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
        public void UpdateSize(Vector2 size)
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
                SetSize(new Vector2(_defSize.x, contentHeight));
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

        /// <summary>
        /// 描边
        /// </summary>
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

                //如果没描过边，则调整节点坐标位置以防止错位
                if (_lastBordered != _isBordered)
                {
                    style.left = _defPos.x - _borderOffset * 0.5f;
                    style.top = _defPos.y - _borderOffset * 0.5f;
                }
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

                //还原节点默认位置
                style.left = _defPos.x;
                style.top = _defPos.y;
            }

            _lastBordered = _isBordered;
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        protected virtual void InitConfig()
        {
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        protected virtual void SetData()
        {
        }

        /// <summary>
        /// 还原数据
        /// </summary>
        protected virtual void ResetData()
        {
        }

        /// <summary>
        /// 自定义UI，如果有复杂的需求无法通过Attribute完成，则可以在这里拓展
        /// </summary>
        protected virtual void CustomUI()
        {
            
        }
    }
}