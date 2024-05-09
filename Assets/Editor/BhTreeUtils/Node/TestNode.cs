namespace BhTreeUtils
{
    public class TestNode : RootNode
    {
        [GraphNode(NodeTypeEnum.Note,"Custom")]
        protected string _desc = "备注";
        [GraphNode(NodeTypeEnum.Input,"测试属性")] private string _test1 = "";
        [GraphNode(NodeTypeEnum.Input,"测试属性")] private string _test2 = "";
        [GraphNode(NodeTypeEnum.Input,"测试属性")] private string _test3 = "";

        protected override void InitConfig()
        {
            title = "测试节点";
            _NodeType = "Test";
        }
    }
}