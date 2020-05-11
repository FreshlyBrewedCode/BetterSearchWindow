using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace BetterSearchWindow.Samples
{
    // This is a basic sample window demonstrating the usage of BetterSearchWindow.
    
    // Start by creating a new class for your custom search window that extends BetterSearchWindow<TWindow, TPayload>
    // where TWindow should just be your class and TPayload is the type of payload you would like to use in the window.
    public class SampleSearchWindow : BetterSearchWindow<SampleSearchWindow, string>
    {
        // This methods creates the list of search tree entries that is displayed in the search window.
        public override List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            // You can just use BetterSearchTree<T> (where T is you payload type) to build your search tree
            return new BetterSearchTree<string>()
                .AddLeaf("Hello/Hello World", "Hello World!")
                .AddLeaf("Hello/Hello There", "Hello there.")
                .AddLeaf("Test", "This is a test.")
                
                // Finally just convert the tree to a list of search tree entries while providing the root name
                // which is just the window title displayed at the root of the search window
                .ToSearchTreeEntries("Sample Window");
        }

        [MenuItem("Better Search Window/Open Sample Window")]
        public static void OpenSampleSearchWindow()
        {
            // Simply open the window using the static method Show while providing a onItemSelectedCallback
            // and the position for the window.
            Show((msg) =>
            {
                Debug.Log(msg);
            }, new Vector2(300, 30));
        }
    }
}

