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
            _root = InitBHNode(json);
        }

        private void Update()
        {
            _root.Run();
        }

        private BhBaseNode InitBHNode(SaveJson json)
        {
            var data = json.data;

            BhBaseNode node = null;

            switch (data.node)
            {
                case "DNodeFail":
                    node = new DNodeFail();
                    break;
                case "DNodeSuccess":
                    node = new DNodeSuccess();
                    break;
                case "DNodeReverse":
                    node = new DNodeReverse();
                    break;
            }

            foreach (var child in json.children)
            {
                node?.AddChild(InitBHNode(child));
            }

            return node;
        }
    }
}