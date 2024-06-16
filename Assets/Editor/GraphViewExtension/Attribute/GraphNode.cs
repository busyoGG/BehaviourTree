using System;

namespace GraphViewExtension
{
    [AttributeUsage(AttributeTargets.Field)]
    public class GraphNode: Attribute
    {
        private NodeTypeEnum _type;

        private string[] _extra;

        public GraphNode(NodeTypeEnum type,params string[] extra)
        {
            _type = type;
            _extra = extra;
        }

        public NodeTypeEnum Type()
        {
            return _type;
        }

        public string[] GetExtra()
        {
            return _extra;
        }
    }
}