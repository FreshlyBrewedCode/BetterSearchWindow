using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BetterSearchWindow
{
    public class BetterSearchTree<T>
    {
        private GUIContent label;
        private List<BetterSearchTree<T>> children;
        private T payload;
        private bool IsLeaf => payload != null;

        public BetterSearchTree()
        {
            children = new List<BetterSearchTree<T>>();
        }
        
        protected BetterSearchTree(GUIContent label)
        {
            this.label = label;
            children = new List<BetterSearchTree<T>>();
        }

        protected BetterSearchTree(GUIContent label, T payload)
        {
            this.label = label;
            this.payload = payload;
        }

        protected GUIContent InheritLabel(string newText)
        {
            return new GUIContent(label)
            {
                text = newText
            };
        }

        public virtual void AddLeaf(GUIContent path, T payload)
        {
            this.label = path;
            Append(payload, path.text);
        }
        
        protected virtual void Append(T destinationPayload, string path)
        {
            // Trim any whitespaces or slashes to get rid of empty entries
            path = path.Trim(' ', '/');

            // Get the position of the first slash, signaling the next entry in the path
            int nextChildIndex = path.IndexOf('/');

            // if there is no slash it means we have reached the end of the path so we just add the payload
            if (nextChildIndex < 0)
            {
                children.Add(new BetterSearchTree<T>(InheritLabel(path), destinationPayload));
                return;
            }

            // Get the name of first entry in the current path
            string childName = path.Substring(0, nextChildIndex);

            // try to get the child entry or create a new one with that name
            var child = GetChild(childName);
            if (child == null)
            {
                child = new BetterSearchTree<T>(InheritLabel(childName));
                children.Add(child);
            }

            // Recursively append the remaining entries
            child.Append(destinationPayload, path.Substring(nextChildIndex + 1));
        }

        // Insert (or merge) another tree as a child into this one
        public void Insert(BetterSearchTree<T> child)
        {
            // Case 1: the provided entry is a leaf and is just added to the children
            if (child.IsLeaf)
            {
                children.Add(child);
                return;
            }

            // Case 2: there is no existing child with the same name as the provided tree
            // so we just add it to the list of children 
            var subChild = GetChild(child.label.text);
            if (subChild == null)
            {
                children.Add(child);
                return;
            }

            // Case 3: We already have a child with the name of the provided tree so we need
            // to insert each sub child individually
            foreach (var c in child.children)
            {
                subChild.Insert(c);
            }
        }
        
        /// <summary>
        /// Get a child of the given name.
        /// </summary>
        /// <param name="name">The name of the child to get</param>
        /// <returns></returns>
        public BetterSearchTree<T> GetChild(string name)
        {
            return children.FirstOrDefault(c => c.label.text == name);
        }
        
        /// <summary>
        /// Convert the tree to a list of search tree entries that can be used by Unity's GraphView SearchWindow.
        /// </summary>
        /// <param name="rootName">The name of the root group. This is the "title" of the search window</param>
        /// <returns></returns>
        public virtual List<SearchTreeEntry> ToSearchTreeEntries(string rootName)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent(rootName), 0));

            foreach (var child in children)
            {
                AddTreeEntry(entries, child, 1);
            }

            return entries;
        }
        
        protected virtual void AddTreeEntry(List<SearchTreeEntry> entries, BetterSearchTree<T> tree, int level)
        {
            // Add Leaf element
            if (tree.IsLeaf)
            {
                var entry = new SearchTreeEntry(tree.label)
                {
                    level = level,
                    userData = tree.payload
                };
                entries.Add(entry);
            }
            // Add Group
            else
            {
                var group = new SearchTreeGroupEntry(new GUIContent(tree.label.text), level);
                entries.Add(group);
        
                // Add children
                foreach (var child in tree.children)
                {
                    AddTreeEntry(entries, child, level + 1);
                }
            }
        }
    }
}