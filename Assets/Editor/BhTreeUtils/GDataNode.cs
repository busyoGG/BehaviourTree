using System.Collections.Generic;
using UnityEngine.NVIDIA;

namespace BhTreeUtils
{
    public class GDataNode
    {
        private List<GDataNode> _children = new List<GDataNode>();

        private dynamic _data;

        public void SetData(dynamic data)
        {
            _data = data;
        }

        public void AddChild(GDataNode node)
        {
            if (!_children.Contains(node))
            {
                _children.Add(node);
            }
        }

        public List<GDataNode> GetChildren()
        {
            return _children;
        }

        public dynamic GetData()
        {
            return _data;
        }
    }
}