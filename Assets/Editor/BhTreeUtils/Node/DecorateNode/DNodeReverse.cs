using UnityEngine;

namespace BhTreeUtils
{
    public class DNodeReverse: RootNode
    {
        [GraphNode(NodeTypeEnum.Note)]
        private string _note = "返回相反的结果";
        protected override void InitConfig()
        {
            _NodeType = "Reverse";
            title = "反转";
            titleContainer.style.backgroundColor = new Color(1.00f, 0.88f, 0.59f,1f);
        }
    }
}