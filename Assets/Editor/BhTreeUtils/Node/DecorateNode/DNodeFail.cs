﻿using System.Dynamic;
using UnityEngine;

namespace BhTreeUtils
{
    public class DNodeFail: RootNode
    {
        [GraphNode(NodeTypeEnum.Note)]
        private string _note = "永远返回Fail";
        protected override void InitConfig()
        {
            _NodeType = "Fail";
            title = "失败";
            titleContainer.style.backgroundColor = new Color(1.00f, 0.88f, 0.59f,1f);
        }
    }
}