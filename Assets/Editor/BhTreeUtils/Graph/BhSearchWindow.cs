using System.Collections.Generic;
using PlasticGui;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BhTree
{
    public class BhSearchWindow: ScriptableObject,ISearchWindowProvider
    {
        public delegate bool SelectHandle(SearchTreeEntry searchTreeEntry,
            SearchWindowContext context);

        public SelectHandle onSelectEntryHandler;

        private bool _inited = false;
        
        public List<SearchTreeEntry> entries = new List<SearchTreeEntry>()
        {
            new SearchTreeGroupEntry(new GUIContent("Create Node"))
        };
        
        private void BuildTree()
        {
            entries.Add(new SearchTreeEntry(new GUIContent("基础节点")){level = 1,userData = typeof(RootNode)});
            _inited = true;
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            if (!_inited)
            {
                BuildTree();
            }
            return entries;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if (onSelectEntryHandler == null)
                return false;
            return onSelectEntryHandler(searchTreeEntry, context);
        }
    }
}