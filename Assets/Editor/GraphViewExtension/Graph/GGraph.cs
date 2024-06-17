using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using Unity.Plastic.Newtonsoft.Json;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GraphViewExtension
{
    public class GGraph : GraphView
    {
        /// <summary>
        /// 所属编辑器
        /// </summary>
        private EditorWindow _editorWindow;

        /// <summary>
        /// 右键创建节点的可搜索多级菜单
        /// </summary>
        private GSearchWindow _seachWindow;

        /// <summary>
        /// 默认节点大小
        /// </summary>
        private Vector2 _defaultNodeSize = new Vector2(200, 102);

        /// <summary>
        /// 当前点击节点
        /// </summary>
        private RootNode _clickNode;

        /// <summary>
        /// 打开的文件路径
        /// </summary>
        private string _filePath = "";

        /// <summary>
        /// 是否打开一个文件
        /// </summary>
        private bool _isOpen = false;

        /// <summary>
        /// 是否在选择
        /// </summary>
        private bool _isSelect = false;

        /// <summary>
        /// 鼠标是否按下
        /// </summary>
        private bool _isDown = false;

        /// <summary>
        /// 鼠标是否移动了
        /// </summary>
        private bool _isMoved = false;

        private Dictionary<string, RootNode> _allNodes = new Dictionary<string, RootNode>();


        public GGraph(EditorWindow editorWindow, GSearchWindow provider)
        {
            _editorWindow = editorWindow;
            _seachWindow = provider;

            //在右键菜单中添加节点
            provider.onSelectEntryHandler = (searchTreeEntry, searchWindowContext) =>
            {
                var windowRoot = _editorWindow.rootVisualElement;
                var windowMousePosition = windowRoot.ChangeCoordinatesTo(windowRoot,
                    searchWindowContext.screenMousePosition - _editorWindow.position.position);
                var graphMousePosition = contentViewContainer.WorldToLocal(windowMousePosition);
                var type = searchTreeEntry.userData as Type;
                CreateNode(type, graphMousePosition);
                return true;
            };

            //监听创建节点
            nodeCreationRequest += context =>
            {
                //打开搜索框
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), provider);
            };

            //监听节点删除
            graphViewChanged += evt =>
            {
                if (evt.elementsToRemove != null)
                {
                    foreach (var element in evt.elementsToRemove)
                    {
                        if (element is RootNode node)
                        {
                            _allNodes.Remove(node.guid);
                        }
                    }
                }

                return evt;
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
            var styleSheet =
                EditorGUIUtility.Load("Assets/Editor/GraphViewExtension/Graph/GridBackground.uss") as StyleSheet;
            styleSheets.Add(styleSheet);
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            SetupToolbar();

            RegisterCallback<PointerDownEvent>(evt =>
            {
                if (evt.button == 0)
                {
                    _isDown = true;
                }
            });

            RegisterCallback<PointerMoveEvent>(evt =>
            {
                if (_isDown)
                {
                    _isMoved = true;
                }
            });

            //监听点击的节点
            RegisterCallback<ClickEvent>(evt =>
            {
                if (!_isMoved)
                {
                    Vector2 localMousePosition = evt.position - new Vector3(layout.xMin, layout.yMin);
                    // 获取当前被选中的对象
                    var currentSelection = panel.Pick(localMousePosition);

                    if (currentSelection == null)
                    {
                        if (_clickNode != null)
                        {
                            _clickNode.UnSelected();
                            _clickNode = null;
                        }
                    }
                    else
                    {
                        bool isInput = false;
                        while (currentSelection != null && currentSelection is not RootNode)
                        {
                            if (IsInputField(currentSelection))
                            {
                                isInput = true;
                                break;
                            }
                            currentSelection = currentSelection.parent;
                        }

                        if (currentSelection == null)
                        {
                            if (_clickNode != null)
                            {
                                _clickNode.UnSelected();
                                _clickNode = null;
                            }
                        }
                        else
                        {
                            if (isInput) return;
                            
                            if (_clickNode != null && currentSelection != _clickNode && currentSelection is not RootNode)
                            {
                                _clickNode.UnSelected();
                                _clickNode = null;
                            }
                            else
                            {
                                _clickNode?.UnSelected();
                                _clickNode = currentSelection as RootNode;
                                _clickNode?.Selected();
                            }
                        }
                    }
                }

                _isMoved = false;
                _isDown = false;
            });

            //监听保存组合键
            RegisterCallback<KeyUpEvent>(OnKeyUp);
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

        public RootNode CreateNode(Type type, Vector2 position)
        {
            RootNode node = Activator.CreateInstance(type) as RootNode;
            //这里只用到了position
            node.SetPosition(new Rect(position, Vector2.zero));
            node.RegistResize(this);
            node.SetSize(_defaultNodeSize);
            AddElement(node);
            _allNodes.Add(node.guid, node);
            return node;
        }

        public RootNode CreateNode(Type type, string guid, Vector2 position, Vector2 size)
        {
            RootNode node = Activator.CreateInstance(type) as RootNode;
            //这里只用到了position
            node.SetPosition(new Rect(position, Vector2.zero));
            node.RegistResize(this);
            node.SetSize(_defaultNodeSize);
            node.UpdateSize(size);
            AddElement(node);
            node.guid = guid;
            _allNodes.Add(guid, node);

            //创建节点标记为打开文件
            _isOpen = true;

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

        public void ClearGraph()
        {
            foreach (var node in _allNodes)
            {
                node.Value.RemoveEdges();
                RemoveElement(node.Value);
            }

            _allNodes.Clear();
            
            _isOpen = false;

            _filePath = "";
        }

        /// <summary>
        /// 是否可输入UI
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        private bool IsInputField(VisualElement element)
        {
            // 判断元素是否是BaseField<T>或其派生类的实例
            return HasBaseField(element.GetType());
        }
        
        bool HasBaseField(Type type)
        {
            if (type.Name.IndexOf("BaseField") != -1)
            {
                return true;
            }
            // 检查当前类型的基类的字段
            if (type.BaseType != null)
            {
                return HasBaseField(type.BaseType);
            }

            return false;
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public List<GDataNode> SaveData()
        {
            List<GDataNode> list = new List<GDataNode>();
            foreach (var nodeData in _allNodes)
            {
                RootNode node = nodeData.Value;

                if (!node.HasParent())
                {
                    GDataNode dataNode = node.SaveData();
                    list.Add(dataNode);
                }
            }

            return list;
        }


        /// <summary>
        /// 当前是否打开文件
        /// </summary>
        /// <returns></returns>
        public bool GetOpen()
        {
            return _isOpen;
        }

        public void SetFilePath(string filePath)
        {
            _filePath = filePath;
        }

        public void OpenData(List<GDataNode> datas, RootNode parent = null)
        {
            foreach (var data in datas)
            {
                Type type = Type.GetType(data.GetNodeType());
                dynamic nodeData = data.GetData();

                string[] pos = nodeData.pos.Trim('(', ')').Split(',');
                string[] size = nodeData.size.Trim('(', ')').Split(',');

                RootNode newNode = CreateNode(type, nodeData.guid,
                    new Vector2(float.Parse(pos[0]), float.Parse(pos[1])),
                    new Vector2(float.Parse(size[0]), float.Parse(size[1])));
                
                newNode.SetData(nodeData);

                if (parent != null)
                {
                    //连线
                    MakeEdge(parent.GetOutput(), newNode.GetInput());
                }

                OpenData(data.GetChildren(), newNode);
            }
        }

        protected virtual void SetupToolbar()
        {
            var toolbar = new Toolbar();
            var openBtn = new ToolbarButton { text = "打开" };
            openBtn.clicked += Open;
            var saveBtn = new ToolbarButton { text = "保存" };
            saveBtn.clicked += Save;
            var closeBtn = new ToolbarButton { text = "关闭" };
            closeBtn.clicked += Close;
            toolbar.Add(openBtn);
            toolbar.Add(saveBtn);
            toolbar.Add(closeBtn);
            Add(toolbar);
        }

        private void Open()
        {
            bool isOpen = GetOpen();
            if (isOpen)
            {
                bool res = EditorUtility.DisplayDialog("打开新文件", "是否打开一个新的文件，当前内容未保存的部分会消失。", "确定", "取消");
                if (res)
                {
                    ClearGraph();
                }
                else
                {
                    return;
                }
            }
            
            string filePath = EditorUtility.OpenFilePanel("打开ScriptableObject", "Assets/Json", "json");

            if (filePath != "")
            {
                string jsonData = "";

                StreamReader sr = File.OpenText(filePath);
                while (sr.ReadLine() is { } nextLine)
                {
                    jsonData += nextLine;
                } 
                sr.Close();
                
                List<SaveJson> json = JsonConvert.DeserializeObject<List<SaveJson>>(jsonData);

                List<GDataNode> list = new List<GDataNode>();

                foreach (var data in json)
                {
                    list.Add(ToGDataNode(data));
                }
                
                SetFilePath(filePath);
                OpenData(list);
            }
        }

        private void Save()
        {
            string filePath = EditorUtility.SaveFilePanel("保存到本地", Application.dataPath + "/Json", "NewFile", "json");
            
            if (filePath != "")
            {
                SetFilePath(filePath);
                
                List<GDataNode> list = SaveData();

                List<SaveJson> listJson = new List<SaveJson>();

                foreach (var data in list)
                {
                    listJson.Add(ToJson(data));
                }

                string jsonData = JsonConvert.SerializeObject(listJson);
                
                FileInfo myFile = new FileInfo(filePath); 
                StreamWriter sw = myFile.CreateText();
       
                foreach (var s in jsonData) 
                { 
                    sw.Write(s); 
                } 
                sw.Close();
                
                _editorWindow.ShowNotification(new GUIContent("保存成功,路径为: " + filePath));
            }
        }

        private void Close()
        {
            ClearGraph();
        }

        private void OnKeyUp(KeyUpEvent evt)
        {
            // 检查是否按下了 Ctrl 键和 S 键
            if (evt.keyCode == KeyCode.S && evt.ctrlKey)
            {
                if (_filePath == "")
                {
                    Save();
                    return;
                }

                Debug.Log("Ctrl + S pressed. Saving...");
                // 执行保存操作
                List<GDataNode> list = SaveData();

                List<SaveJson> listJson = new List<SaveJson>();

                foreach (var data in list)
                {
                    listJson.Add(ToJson(data));
                }

                string jsonData = JsonConvert.SerializeObject(listJson);

                FileInfo myFile = new FileInfo(_filePath);
                StreamWriter sw = myFile.CreateText();

                foreach (var s in jsonData)
                {
                    sw.Write(s);
                }

                sw.Close();
                // 这里可以添加你的保存逻辑
                evt.StopPropagation(); // 阻止事件传递，避免触发其他事件
            }
        }

        /// <summary>
        /// 转换为JSON
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public SaveJson ToJson(GDataNode data)
        {
            SaveJson save = new SaveJson();
            save.type = data.GetNodeType();
            save.data = data.GetData();

            foreach (var child in data.GetChildren())
            {
                save.children.Add(ToJson(child));
            }

            return save;
        }

        /// <summary>
        /// 转换为节点数据
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public GDataNode ToGDataNode(SaveJson json)
        {
            GDataNode data = new GDataNode();
            data.SetNodeType(json.type);

            dynamic obj = new ExpandoObject();

            foreach (var res in (json.data as JObject).Properties())
            {
                JTokenType jType = res.Value.Type;
                switch (jType)
                {
                    case JTokenType.String:
                        ((IDictionary<string, object>)obj)[res.Name] = res.Value.Value<string>();
                        break;
                    case JTokenType.Float:
                        ((IDictionary<string, object>)obj)[res.Name] = res.Value.Value<float>();
                        break;
                    case JTokenType.Integer:
                        ((IDictionary<string, object>)obj)[res.Name] = res.Value.Value<int>();
                        break;
                    case JTokenType.Boolean:
                        ((IDictionary<string, object>)obj)[res.Name] = res.Value.Value<bool>();
                        break;
                }
            }

            data.SetData(obj);

            foreach (var child in json.children)
            {
                data.AddChild(ToGDataNode(child));
            }

            return data;
        }
    }
}