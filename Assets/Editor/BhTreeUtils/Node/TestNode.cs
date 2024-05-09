namespace BhTreeUtils
{
    public class TestNode : RootNode
    {
        [GraphNode(NodeTypeEnum.Note,"Custom")]
        protected string _desc = "备注";
        [GraphNode(NodeTypeEnum.Input,"测试属性")] private string _test1 = "";
        [GraphNode(NodeTypeEnum.Enum)] private NodeTypeEnum _enum = NodeTypeEnum.Label;

        [GraphNode(NodeTypeEnum.Slide,"浮点滑动条", "0", "150")]
        private float _slider = 0;
        
        [GraphNode(NodeTypeEnum.Slide,"整型滑动条","Int", "0", "1")]
        private float _sliderInt = 0;

        protected override void InitConfig()
        {
            title = "测试节点";
            _NodeType = "Test";
        }
    }
}