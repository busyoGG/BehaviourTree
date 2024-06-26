using System.Collections.Generic;

namespace BhTree
{
    public class BhBaseNode
    {
        public int id;
        
        protected BhResult result;

        private BhBaseNode _parent;

        protected List<BhBaseNode> children = new List<BhBaseNode>();
        
        protected int currentChildIndex;

        /// <summary>
        /// 是否允许中断，允许中断的情况下每次都会从第一个子节点开始检测
        /// </summary>
        protected bool interruptCheck = false;
        

        public void SetResult(BhResult res)
        {
            result = res;
        }

        public BhResult GetResult()
        {
            return result;
        }

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="node"></param>
        public void AddChild(BhBaseNode node)
        {
            children.Add(node);
            node._parent = this;
        }

        /// <summary>
        /// 获取所有子节点
        /// </summary>
        /// <returns></returns>
        public List<BhBaseNode> GetChildren()
        {
            return children;
        }

        /// <summary>
        /// 获取当前子节点索引
        /// </summary>
        /// <returns></returns>
        public int GetCurIndex()
        {
            return currentChildIndex;
        }

        public void SetCurIndex(int index)
        {
            currentChildIndex = index;
        }

        /// <summary>
        /// 获取中断检测状态
        /// </summary>
        /// <returns></returns>
        public bool GetInterruptCheck()
        {
            return interruptCheck;
        }

        /// <summary>
        /// 获取父节点
        /// </summary>
        /// <returns></returns>
        public BhBaseNode GetParent()
        {
            return _parent;
        }

        public virtual void Init(dynamic data)
        {
            
        }

        public virtual void Run()
        {
        }

        /// <summary>
        /// 检测运行结果是否会终止执行状态
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        public virtual bool CheckState(BhResult res)
        {
            return true;
        }

        public virtual bool CheckStop()
        {
            return true;
        }
        
        public virtual void Reset()
        {
            currentChildIndex = 0;
            result = BhResult.Running;
            foreach (var child in children)
            {
                child.Reset();
            }
        }
    }
}
