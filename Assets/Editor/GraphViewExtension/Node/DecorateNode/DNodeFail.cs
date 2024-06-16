using System.Dynamic;
using UnityEngine;

namespace GraphViewExtension
{
    public class DNodeFail: RootNode
    {
        [GraphNode(NodeTypeEnum.Note)]
        private string _note = "永远返回Fail";
        protected override void InitConfig()
        {
            title = "失败";
            titleContainer.style.backgroundColor = new Color(1.00f, 0.88f, 0.59f,1f);
        }

        protected override void SetData()
        {
            _data.node = "DNodeFail";
        }
    }
}