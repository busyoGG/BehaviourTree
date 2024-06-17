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

        private Dictionary<string, BhBaseNode> _roots = new Dictionary<string, BhBaseNode>();
        
        private Dictionary<string, BhBaseNode> _curNode = new Dictionary<string, BhBaseNode>();

        private Dictionary<string, object> _blackboard = new Dictionary<string, object>();

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

            var children = node.GetChildren();

            while (children.Count > 0)
            {
                node = children[0];
                children = node.GetChildren();
            }

            RunNode(node);
        }

        private void RunNode(BhBaseNode node)
        {
            if (node == null) return;

            var children = node.GetChildren();

            if (children.Count > 0)
            {
                
                int index = node.GetCurIndex();

                children[index].Run();
                BhResult state = children[index].GetResult();
                index++;
            
                Debug.Log("子节点运行结果" + state);

                //先检测子节点运行状态对当前节点的状态是否有影响，然后检测是否完全运行完子节点
                while (node.CheckState(state) && index < children.Count)
                {
                    children[index].Run();
                    state = children[index].GetResult();
                    index++;
                
                    Debug.Log("子节点运行结果" + state);
                }
            
                //设置当前节点运行的子对象索引
                node.SetCurIndex(index);
            
                //设置当前节点状态
                node.SetResult(state);
            }
            
            if (node.CheckStop())
            {
                RunNode(node.GetParent());
                node.Reset();
            }
        }

        public void SetBlackboard(string key, object data)
        {
            _blackboard[key] = data;
        }

        public object GetBlackboard(string key)
        {
            _blackboard.TryGetValue(key, out var data);
            return data;
        }
    }
}