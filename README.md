# Better Search Window
[![openupm](https://img.shields.io/npm/v/com.freshlybrewedcode.bettersearchwindow?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/com.freshlybrewedcode.bettersearchwindow/)

A simple extension/wrapper to simplify the usage of the [UnityEditor.Experimental.GraphView.SearchWindow](https://docs.unity3d.com/ScriptReference/Experimental.GraphView.SearchWindow.html) API.

![bettersearchwindow-showcase](https://i.imgur.com/58Og2w5.gif)

## Usage
1. Create a new search window class that inherits from `BetterSearchWindow<TWindow, TPayload>`:

```C#
using BetterSearchWindow;

// TWindow should just be your class and TPayload is the type of payload you would like to use in the window.
public class SampleSearchWindow : BetterSearchWindow<SampleSearchWindow, string>
{
}
```
2. Implement `CreateSearchTree(SearchWindowContext context)` using `BetterSearchTree<TPayload>` to create the `List<SearchTreeEntry>`:

```C#
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
```
3. Provide a callback method that should be called when a leaf entry is selected:

```C#
// Just a method that receives the payload of the selected entry
private static void MyItemSelectedCallback(string payload)
{
    Debug.Log(payload);
}
```
4. Show the window using the static `Show(Action<TPayload> callback, Vector2 position)` method:

```C#
// Position is optional. It will try to fallback to the mouse position if nothing else is specified
SampleSearchWindow.Show(MyItemSelectedCallback, new Vector2(300, 30));
```

## Install
### Open UPM
The package is available on the [openupm registry](https://openupm.com/). You can install it via openupm-cli.
```
openupm add com.freshlybrewedcode.bettersearchwindow
```

### Git Url
Open the Package Manager in Unity (Window/Package Manager), on the top left click on the plus icon and select "Add package from git URL...", enter the following url and click "Add"
```
https://github.com/FreshlyBrewedCode/BetterSearchWindow.git#upm
```

### Download
Go to the [releases tab](https://github.com/FreshlyBrewedCode/BetterSearchWindow/releases) of this repository and download the source code for the latest release. Place the "Better Search Window" directory into your projects Assets or Packages folder.
