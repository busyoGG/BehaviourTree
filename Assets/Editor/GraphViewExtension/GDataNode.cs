using System.Collections.Generic;

namespace GraphViewExtension
{
    public class GDataNode
    {
        private List<GDataNode> _children = new List<GDataNode>();

        private dynamic _data;

        private string _type;

        public void SetData(dynamic data)
        {
            _data = data;
        }

        public void SetNodeType(string type)
        {
            _type = type;
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

        public string GetNodeType()
        {
            return _type;
        }
    }
}