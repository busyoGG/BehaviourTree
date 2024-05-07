namespace BhTreeUtils
{
    public class TestNode : RootNode
    {
        [GraphNode(NodeTypeEnum.Note,"Custom")]
        protected string _desc = "��ע";
        [GraphNode(NodeTypeEnum.Input,"��������")] private string _test1 = "";
        [GraphNode(NodeTypeEnum.Input,"��������")] private string _test2 = "";
        [GraphNode(NodeTypeEnum.Input,"��������")] private string _test3 = "";

        protected override void InitConfig()
        {
            title = "���Խڵ�";
            _NodeType = "Test";
        }
    }
}