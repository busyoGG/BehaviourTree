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

            if (curNode == null)
            {
                var children = node.GetChildren();

                while (children.Count > 0)
                {
                    node = children[0];
                    children = node.GetChildren();
                }
            }
            else
            {
                node = curNode;
            }

            RunNode(node, root);
        }

        private void RunNode(BhBaseNode node, BhBaseNode root)
        {
            if (node == null)
            {
                //一次执行完成
                _curNode[root] = null;
                return;
            }

            var children = node.GetChildren();

            BhBaseNode child = null;
            
            if (children.Count > 0)
            {
                
                int index = node.GetCurIndex();

                child = children[index];
                
                child.Run();
                
                BhResult state = child.GetResult();
                //设置当前节点运行的子对象索引
                node.SetCurIndex(index++);
                
                Debug.Log("子节点" + child + "运行结果 " + state);


                //先检测子节点运行状态对当前节点的状态是否有影响，然后检测是否完全运行完子节点
                while (node.CheckState(state) && index < children.Count)
                {
                    child = children[index];
                    child.Run();
                    state = child.GetResult();
                    //设置当前节点运行的子对象索引
                    node.SetCurIndex(index++);
                    Debug.Log("子节点" + child + "运行结果 " + state);
                }


                //设置当前节点状态
                node.SetResult(state);
            }

            if (node.CheckStop())
            {
                RunNode(node.GetParent(), root);
                node.Reset();
                foreach (var c in children)
                {
                    c.Reset();
                }
            }
            else
            {
                Debug.Log(node + "没执行完");
                _curNode[root] = node;
            }
        }
    }
}