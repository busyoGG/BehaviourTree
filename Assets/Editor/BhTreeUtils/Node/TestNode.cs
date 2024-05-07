namespace BhTreeUtils
{
    public class TestNode : RootNode
    {
        [GraphNode(NodeTypeEnum.Note,"Custom")]
        protected string _desc = "±∏◊¢";
        [GraphNode(NodeTypeEnum.Input,"≤‚ ‘ Ù–‘")] private string _test1 = "";
        [GraphNode(NodeTypeEnum.Input,"≤‚ ‘ Ù–‘")] private string _test2 = "";
        [GraphNode(NodeTypeEnum.Input,"≤‚ ‘ Ù–‘")] private string _test3 = "";

        protected override void InitConfig()
        {
            title = "≤‚ ‘Ω⁄µ„";
            _NodeType = "Test";
        }
    }
}