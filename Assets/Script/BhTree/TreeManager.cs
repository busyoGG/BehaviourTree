using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BhTree
{
    public class TreeManager : Singleton<TreeManager>
    {
        /// <summary>
        /// 节点编号
        /// </summary>
        private int _id = 0;

        private Dictionary<BhBaseNode, BhBaseNode> _curNode = new Dictionary<BhBaseNode, BhBaseNode>();

        /// <summary>
        /// 创建行为树
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
        public BhBaseNode InitBHNode(SaveJson json)
        {
            var data = json.data;

            string typeName = "BhTree." + data.node;

            Type type = Type.GetType(typeName);

            BhBaseNode node = Activator.CreateInstance(type) as BhBaseNode;

            //初始化数据
            node?.Init(data);

            node.id = _id++;

            foreach (var child in json.children)
            {
                node?.AddChild(InitBHNode(child));
            }

            return node;
        }

        public void Run(BhBaseNode node)
        {
            if (node == null) return;

            BhBaseNode root = node;

            BhBaseNode curNode;

            _curNode.TryGetValue(node, out curNode);

            if (curNode != null)
            {
                node = curNode;
            }

            RunNode(node, root);
        }

        private void RunNode(BhBaseNode node, BhBaseNode root,BhBaseNode parent = null)
        {
            if (node == null)
            {
                Debug.Log("一次执行结束");
                _curNode[root] = null;
                root.Reset();
                return;
            }

            if (node == parent)
            {
                return;
            }
            
            var children = node.GetChildren();

            if (children.Count != 0)
            {
                bool interrupt = node.GetInterruptCheck();
                int index = interrupt && node.GetResult() == BhResult.Running ? 0 : node.GetCurIndex();
                BhResult res;

                for (int i = index; i < children.Count; i++)
                {
                    var child = children[i];
                    RunNode(child,root,node);
                    
                    child.Run();
                    res = child.GetResult();
                    
                    if (!node.CheckState(res) || i == children.Count - 1)
                    {
                        node.SetCurIndex(i);
                        node.SetResult(res);
                        break;
                    }
                }

                if (node.CheckStop())
                {
                    RunNode(node.GetParent(), root,parent);
                }
                else
                {
                    if (!interrupt)
                    {
                        _curNode[root] = node;
                    }
                    else
                    {
                        //如果当前节点可打断，向上查找到最后一个能打断的节点并存入
                        BhBaseNode baseNode = node;
                        BhBaseNode tempNode = null;
                        while (baseNode.GetInterruptCheck())
                        {
                            tempNode = baseNode;
                            baseNode = baseNode.GetParent();
                        }
                    
                        _curNode[root] = tempNode;
                    }
                }
            }
        }

    }
}