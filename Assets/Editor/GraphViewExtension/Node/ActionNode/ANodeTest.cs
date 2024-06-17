using UnityEngine;

namespace GraphViewExtension
{
    public class ANodeTest: RootNode
    {
        [GraphNode(NodeTypeEnum.Note,"Custom")]
        private string _note = "测试节点";

        [GraphNode(NodeTypeEnum.Toggle),GName("是否返回Success")]
        private bool _isSuc;
        protected override void InitConfig()
        {
            title = "测试";
            titleContainer.style.backgroundColor = new Color(0.0f, 0.8f, 0.8f,1f);
        }

        protected override void ResetData()
        {
            _isSuc = _data.isSuc;
            _note = _data.desc;
        }

        protected override void SetData()
        {
            _data.node = "ANodeTest";
            _data.isSuc = _isSuc;
            _data.desc = _note;
        }
    }
}