#if UNITY_2019_1_OR_NEWER
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BetterSearchWindow
{
    /// <summary>
    /// Non-Generic base class for the BetterSearchWindow
    /// </summary>
    public abstract class BetterSearchWindow : ScriptableObject
    {
        public abstract void Open<T>(Action<T> onItemSelectedCallback, Vector2 position);
    }
    
    /// <summary>
    /// Generic base class for a custom BetterSearchWindow.
    /// </summary>
    /// <typeparam name="TWindow">The type of window used for the "current" window. This should just be the type that
    /// inherits from BetterSearchWindow.</typeparam>
    /// <typeparam name="TPayload">The type of payload that should be used in the search window. This type will
    /// be passed to the <see cref="onItemSelected"/> callback.</typeparam>
    public abstract class BetterSearchWindow<TWindow, TPayload> : BetterSearchWindow, ISearchWindowProvider
        where TWindow : BetterSearchWindow
    {
        // Manage a static instance of the window so we can just open it from anywhere
        private static TWindow current;
        private static TWindow Current
        {
            get
            {
                if (!current) current = FindObjectOfType<TWindow>();
                if(!current) current = CreateInstance<TWindow>();
                return current;
            }
        }

        // The callback for when the user selects an item in the search window
        protected Action<TPayload> onItemSelected;
        
        /// <summary>
        /// Open the search window at the given screen position
        /// </summary>
        /// <param name="onItemSelectedCallback">The callback for when the user selects an item in the search window.</param>
        /// <param name="position">The position at which the window should appear.</param>
        /// <typeparam name="T">The payload type used for the callback.</typeparam>
        public override void Open<T>(Action<T> onItemSelectedCallback, Vector2 position)
        {
            this.onItemSelected = onItemSelectedCallback as Action<TPayload>;
            SearchWindow.Open(new SearchWindowContext(position), this);
        }
        
        /// <summary>
        /// Open the search window at the current mouse position. Note that this might not work depending on the
        /// context since it relies on Event.current with could be null in some cases.
        /// </summary>
        /// <param name="onItemSelectedCallback"></param>
        /// <typeparam name="T"></typeparam>
        public virtual void Open<T>(Action<T> onItemSelectedCallback)
        {
            Vector2 pos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Open(onItemSelectedCallback, pos);
        }
        
        /// <summary>
        /// <inheritdoc cref="Open{T}(System.Action{T},UnityEngine.Vector2)"/>
        /// </summary>
        /// <param name="onItemSelectedCallback"></param>
        /// <param name="position"></param>
        public static void Show(Action<TPayload> onItemSelectedCallback, Vector2 position)
        {
            Current.Open(onItemSelectedCallback, position);
        }
        
        /// <summary>
        /// <inheritdoc cref="Open{T}(System.Action{T})"/>
        /// </summary>
        /// <param name="onItemSelectedCallback"></param>
        public static void Show(Action<TPayload> onItemSelectedCallback)
        {
            Vector2 pos = GUIUtility.GUIToScreenPoint(Event.current.mousePosition);
            Show(onItemSelectedCallback, pos);
        }
        
        // Implement ISearchWindowProvider...
        
        public abstract List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context);
        
        public virtual bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            // If there is no user data we don't have a payload to call onItemSelected
            if (searchTreeEntry.userData == null) return false;
            
            // Invoke the callback with the user data payload
            onItemSelected?.Invoke((TPayload)searchTreeEntry.userData);
            return true;
        }
    }

    public class FriendlySearchWindowTree<T>
    {
        public GUIContent label;
        public List<FriendlySearchWindowTree<T>> children;
        public T payload;
        public bool IsLeaf => payload != null;

        public FriendlySearchWindowTree()
        {
            children = new List<FriendlySearchWindowTree<T>>();
        }
        
        private FriendlySearchWindowTree(GUIContent label)
        {
            this.label = label;
            children = new List<FriendlySearchWindowTree<T>>();
        }

        private FriendlySearchWindowTree(GUIContent label, T payload)
        {
            this.label = label;
            this.payload = payload;
        }

        public GUIContent InheritLabel(string newText)
        {
            return new GUIContent(label)
            {
                text = newText
            };
        }

        public void AddLeaf(GUIContent path, T payload)
        {
            this.label = path;
            Append(payload, path.text);
        }
        
        private void Append(T destinationPayload, string path)
        {
            // Trim any whitespaces or slashes to get rid of empty entries
            path = path.Trim(' ', '/');

            // Get the position of the first slash, signaling the next entry in the path
            int nextChildIndex = path.IndexOf('/');

            // if there is no slash it means we have reached the end of the path so we just add the payload
            if (nextChildIndex < 0)
            {
                children.Add(new FriendlySearchWindowTree<T>(InheritLabel(path), destinationPayload));
                return;
            }

            // Get the name of first entry in the current path
            string childName = path.Substring(0, nextChildIndex);

            // try to get the child entry or create a new one with that name
            var child = GetChild(childName);
            if (child == null)
            {
                child = new FriendlySearchWindowTree<T>(InheritLabel(childName));
                children.Add(child);
            }

            // Recursively append the remaining entries
            child.Append(destinationPayload, path.Substring(nextChildIndex + 1));
        }

        // Insert (or merge) another tree as a child into this one
        public void Insert(FriendlySearchWindowTree<T> child)
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
        public FriendlySearchWindowTree<T> GetChild(string name)
        {
            return children.FirstOrDefault(c => c.label.text == name);
        }
        
        /// <summary>
        /// Convert the tree to a list of search tree entries that can be used by Unity's GraphView SearchWindow.
        /// </summary>
        /// <param name="rootName">The name of the root group. This is the "title" of the search window</param>
        /// <returns></returns>
        public List<SearchTreeEntry> ToSearchTreeEntries(string rootName)
        {
            var entries = new List<SearchTreeEntry>();
            entries.Add(new SearchTreeGroupEntry(new GUIContent(rootName), 0));

            foreach (var child in children)
            {
                AddTreeEntry(entries, child, 1);
            }

            return entries;
        }
        
        private void AddTreeEntry(List<SearchTreeEntry> entries, FriendlySearchWindowTree<T> tree, int level)
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

    /// <summary>
    /// Static class that provides utility methods related to the BetterSearchWindow
    /// </summary>
    public static class FriendlySearchWindowUtility
    {
        public struct TypeWithAttributes<T> where T : Attribute
        {
            public Type type;
            public IEnumerable<T> attributes;
        }
        
        /// <summary>
        /// Get a list of types that have a attribute of type <see cref="T"/> attached.
        /// </summary>
        /// <typeparam name="T">The attribute type to search for</typeparam>
        /// <returns>A list of types along with the attribute.</returns>
        public static List<TypeWithAttributes<T>> GetTypesByAttribute<T>() where T : System.Attribute
        {
            // Just a fancy LINQ query...
            var typesWithAttribute =
                from a in AppDomain.CurrentDomain.GetAssemblies().AsParallel()
                from t in a.GetTypes()
                let attributes = t.GetCustomAttributes(typeof(T), true)
                where attributes != null && attributes.Length > 0
                select new TypeWithAttributes<T>() { type = t, attributes = attributes.Cast<T>() };
            
            return typesWithAttribute.ToList();
        } 
    }
}
#endif