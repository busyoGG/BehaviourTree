using UnityEngine.UIElements;

namespace GraphViewExtension
{
    public class TestNode : RootNode
    {
        [GraphNode(NodeTypeEnum.Note, "Custom")]
        protected string _desc = "备注";

        [GraphNode(NodeTypeEnum.Input), GName("测试输入")]
        private string _test1 = "";

        [GraphNode(NodeTypeEnum.Enum), GName("测试枚举")]
        private NodeTypeEnum _enum = NodeTypeEnum.Label;

        [GraphNode(NodeTypeEnum.Slide, "0", "150"), GName("浮点滑动条")]
        private float _slider = 0;

        [GraphNode(NodeTypeEnum.Slide, "Int", "0", "1"), GName("整型滑动条")]
        private float _sliderInt = 0;

        [GraphNode(NodeTypeEnum.Radio, "选项1", "选项2")]
        private int _radio;


        [GraphNode(NodeTypeEnum.Box, "2")] private string _box = "";

        [GraphNode(NodeTypeEnum.Toggle, "选择1"), GWidth(100, LengthUnit.Pixel)]
        private bool _toggle1;

        [GraphNode(NodeTypeEnum.Toggle, "选择2"), GWidth(100, LengthUnit.Pixel)]
        private bool _toggle2;

        [GraphNode(NodeTypeEnum.Toggle, "选择3")]
        private bool _toggle3;

        protected override void InitConfig()
        {
            title = "测试节点";
        }

        protected override void SetData()
        {
            _data.desc = _desc;
        }

        protected override void ResetData()
        {
            _desc = _data.desc;
        }
    }
}