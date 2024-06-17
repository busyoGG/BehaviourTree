using System;
using BhTree;
using UnityEngine;

namespace Game
{
    public class Launcher: MonoBehaviour
    {
        private BhBaseNode _root;
        
        private void Start()
        {
            SaveJson json = ConfigLoader.Load();
            _root = TreeManager.Ins().InitBHNode(json);
            
            // TreeManager.Ins().Run(_root);
        }

        private void Update()
        {
            TreeManager.Ins().Run(_root);
        }
    }
}