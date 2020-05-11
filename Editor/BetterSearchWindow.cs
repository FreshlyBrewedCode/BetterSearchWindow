#if UNITY_2019_1_OR_NEWER
using System;
using System.Collections.Generic;
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
}
#endif