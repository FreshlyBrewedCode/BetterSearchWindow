using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.IMGUI.Controls;
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

        public virtual BetterSearchTree<T> AddLeaf(GUIContent path, T payload)
        {
            this.label = path;
            Append(payload, path.text);
            return this;
        }

        public virtual BetterSearchTree<T> AddLeaf(string path, T payload)
        {
            return AddLeaf(new GUIContent(path), payload);
        }
        
        protected virtual void Append(T destinationPayload, string path)
        {
            // Trim any whitespaces or slashes to get rid of empty entries
            // path = path.Trim(' ', '/');
            bool isEnd;
            string name;
            string nextPath = StepPath(path, out isEnd, out name);
            
            // Get the position of the first slash, signaling the next entry in the path
            // int nextChildIndex = path.IndexOf('/');

            // if there is no slash it means we have reached the end of the path so we just add the payload
            if (isEnd)
            {
                children.Add(new BetterSearchTree<T>(InheritLabel(name), destinationPayload));
                return;
            }

            // Get the name of first entry in the current path
            // string childName = path.Substring(0, nextChildIndex);

            // try to get the child entry or create a new one with that name
            var child = GetChild(name);
            if (child == null)
            {
                child = new BetterSearchTree<T>(InheritLabel(name));
                children.Add(child);
            }

            // Recursively append the remaining entries
            child.Append(destinationPayload, nextPath);
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

        public BetterSearchTree<T> GetNestedChild(string path)
        {
            bool isEnd;
            string name;
            string nextPath = StepPath(path, out isEnd, out name);
            var child = GetChild(name);
            if (child == null) return null;

            if (isEnd) return child;
            return child.GetNestedChild(nextPath);
        }

        public virtual BetterSearchTree<T> SetIcon(string path, Texture2D icon)
        {
            var item = GetNestedChild(path);
            if (item != null) item.label.image = icon;
            return this;
        }
        
        public virtual BetterSearchTree<T> SetIcon(string path, string icon)
        {
            return SetIcon(path, GetIcon(icon));
        }

        protected virtual Texture2D GetIcon(string iconName)
        {
            return EditorGUIUtility.IconContent(iconName).image as Texture2D;
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

        public virtual AdvancedDropdownItem<T> ToAdvancedDropdown(string rootName)
        {
            var root = new AdvancedDropdownItem<T>(rootName);
            foreach (var child in children)
            {
                child.AddAdvancedDropdownChildren(root);
            }
            return root;
        }

        public void ShowAsAdvancedDropdown(Rect buttonRect, string rootName, Action<T> onItemSelectedCallback)
        {
            BetterAdvancedDropdown<T>.Show(buttonRect, ToAdvancedDropdown(rootName), onItemSelectedCallback);
        }
        
        public void ShowAsAdvancedDropdown(Rect buttonRect, Vector2 minSize, string rootName, Action<T> onItemSelectedCallback)
        {
            BetterAdvancedDropdown<T>.Show(buttonRect, minSize,ToAdvancedDropdown(rootName), onItemSelectedCallback);
        }
        
        protected virtual void AddAdvancedDropdownChildren(AdvancedDropdownItem<T> parent)
        {
            var item = new AdvancedDropdownItem<T>(label.text);

            if (IsLeaf)
            {
                item.icon = label.image as Texture2D;
                item.payload = payload;
                parent.AddChild(item);
                return;
            }
            
            foreach (var child in children)
            {
                child.AddAdvancedDropdownChildren(item);
            }
            parent.AddChild(item);
        }

        protected string StepPath(string path, out bool isEnd, out string name)
        {
            // Trim any whitespaces or slashes to get rid of empty entries
            path = path.Trim(' ', '/');

            // Get the position of the first slash, signaling the next entry in the path
            int nextChildIndex = path.IndexOf('/');

            // if there is no slash it means we have reached the end of the path
            if (nextChildIndex < 0)
            {
                isEnd = true;
                name = path;
                return string.Empty;
            }

            // Get the name of first entry in the current path
            name = path.Substring(0, nextChildIndex);
            isEnd = false;
            return path.Substring(nextChildIndex + 1);
        }
    }
}