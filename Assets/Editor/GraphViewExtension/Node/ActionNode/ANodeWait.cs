using UnityEngine;

namespace GraphViewExtension
{
    public class ANodeWait: RootNode
    {
        [GraphNode(NodeTypeEnum.Note,"Custom")]
        private string _note = "延时节点";

        [GraphNode(NodeTypeEnum.Slide,"Int","0","10000"),GName("延时时间")]
        private int _time;
        protected override void InitConfig()
        {
            title = "延时";
            titleContainer.style.backgroundColor = new Color(0.0f, 0.8f, 0.8f,1f);
        }

        protected override void ResetData()
        {
            _time = _data.time;
            _note = _data.desc;
        }

        protected override void SetData()
        {
            _data.node = "ANodeWait";
            _data.time = _time;
            _data.desc = _note;
        }
    }
}