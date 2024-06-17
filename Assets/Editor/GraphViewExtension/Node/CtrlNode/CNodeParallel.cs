using System.Dynamic;
using UnityEngine;

namespace GraphViewExtension
{
    public class CNodeParallel: RootNode
    {
        [GraphNode(NodeTypeEnum.Note)]
        private string _note = "并行节点";
        protected override void InitConfig()
        {
            title = "并行";
            titleContainer.style.backgroundColor = new Color(0.0f, 0.8f, 0.0f,1f);
        }

        protected override void SetData()
        {
            _data.node = "CNodeParallel";
        }
    }
}