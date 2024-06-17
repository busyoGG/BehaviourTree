using System.Dynamic;
using UnityEngine;

namespace GraphViewExtension
{
    public class CNodeSequence: RootNode
    {
        [GraphNode(NodeTypeEnum.Note)]
        private string _note = "顺序节点，有一个为 False 就返回";
        protected override void InitConfig()
        {
            title = "顺序";
            titleContainer.style.backgroundColor = new Color(0.0f, 0.8f, 0.0f,1f);
        }

        protected override void SetData()
        {
            _data.node = "CNodeSequence";
        }
    }
}