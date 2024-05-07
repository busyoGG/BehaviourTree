using UnityEngine;

namespace BhTreeUtils
{
    public class DNodeSuccess: RootNode
    {
        [GraphNode(NodeTypeEnum.Note)]
        private string _note = "永远返回Success";
        protected override void InitConfig()
        {
            _NodeType = "Success";
            title = "成功";
            titleContainer.style.backgroundColor = new Color(1.00f, 0.88f, 0.59f,1f);
        }
    }
}