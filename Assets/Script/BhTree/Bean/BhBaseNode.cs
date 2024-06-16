using System.Collections.Generic;

namespace BhTree
{
    public class BhBaseNode
    {
        private BhResult _result;

        private BhBaseNode _parent;

        protected List<BhBaseNode> _children = new List<BhBaseNode>();

        protected void SetResult(BhResult result)
        {
            _result = result;
        }

        protected virtual BhResult GetResult()
        {
            return _result;
        }

        public void AddChild(BhBaseNode node)
        {
            _children.Add(node);
            node._parent = this;
        }

        public BhResult Run()
        {
            return GetResult();
        }
    }
}
