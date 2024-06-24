using System.Dynamic;
using UnityEngine;

namespace GraphViewExtension
{
    public class CNodeSelect: RootNode
    {
        [GraphNode(NodeTypeEnum.Note)]
        private string _note = "选择节点，有一个为 True 就返回";
        [GraphNode(NodeTypeEnum.Toggle)]
        private bool _interrupt = false;
        protected override void InitConfig()
        {
            title = "选择";
            titleContainer.style.backgroundColor = new Color(0.0f, 0.8f, 0.0f,1f);
        }

        protected override void ResetData()
        {
            _interrupt = _data.interrupt;
        }

        protected override void SetData()
        {
            _data.node = "CNodeSelect";
            _data.interrupt = _interrupt;
        }
    }
}