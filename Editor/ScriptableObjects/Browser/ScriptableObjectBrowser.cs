using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Moths.ScriptableObjects.Browser
{
    public struct SearchFilter
    {
        private string _filter;

        public static implicit operator SearchFilter(string filter)
        {
            return new SearchFilter { _filter = filter.ToLower() };
        }

        public static bool operator !=(SearchFilter filter, string text) => filter._filter != text;
        public static bool operator ==(SearchFilter filter, string text) => filter._filter == text;

        public bool Apply(string text)
        {
            return (string.IsNullOrEmpty(_filter) || text.ToLower().Contains(_filter));
        }
    }

    public class ScriptableObjectBrowser : EditorWindow
    {
        [MenuItem("Window/ScriptableObject Browser")]
        public static void Open()
        {
            var wnd = GetWindow<ScriptableObjectBrowser>();
            wnd.titleContent = new GUIContent("SO Browser");
            wnd.Show();
        }

        TreeViewState rootViewState;
        RootFolderTreeView rootTreeView;

        TreeViewState treeViewState;
        SOAssetTreeView treeView;

        SearchField searchField;

        string rootPath;

        string searchString = "";

        int selectedTab = 0; // 0 = All, 1 = Favourites

        void OnEnable()
        {
            var treeViewStateJson = EditorUserSettings.GetConfigValue("Moths/ScriptableObjectBrowser/treeViewState");
            var rootTreeViewStateJson = EditorUserSettings.GetConfigValue("Moths/ScriptableObjectBrowser/rootViewState");
            rootPath = EditorUserSettings.GetConfigValue("Moths/ScriptableObjectBrowser/rootPath");
            
            treeViewState = string.IsNullOrEmpty(treeViewStateJson) ? new TreeViewState() : JsonUtility.FromJson<TreeViewState>(treeViewStateJson);
            rootViewState = string.IsNullOrEmpty(rootTreeViewStateJson) ? new TreeViewState() : JsonUtility.FromJson<TreeViewState>(rootTreeViewStateJson);

            treeView = new SOAssetTreeView(treeViewState, SOEditorUtility.Types.ToArray());
            treeView.searchString = "";

            rootTreeView = new RootFolderTreeView(this, rootViewState);
            rootTreeView.searchString = "";
            
            Reload();

            if (string.IsNullOrEmpty(rootPath)) rootPath = "";

            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
        }

        private void OnDisable()
        {
            EditorUserSettings.SetConfigValue("Moths/ScriptableObjectBrowser/treeViewState", JsonUtility.ToJson(treeViewState));
            EditorUserSettings.SetConfigValue("Moths/ScriptableObjectBrowser/rootViewState", JsonUtility.ToJson(rootViewState));
            EditorUserSettings.SetConfigValue("Moths/ScriptableObjectBrowser/rootPath", rootPath ?? "");
        }//

        void OnGUI()
        {
            bool isChanged = false;
            int newTab = GUILayout.Toolbar(selectedTab, new[] { "All", "Favourites" });

            isChanged = newTab != selectedTab;
            selectedTab = newTab;

            if (selectedTab == 0)
            {
                treeView.isFavourites = false;
                rootTreeView.isFavourites = false;
            }
            else if (selectedTab == 1)
            {
                treeView.isFavourites = true;
                rootTreeView.isFavourites = true;
            }

            if (isChanged)
            {
                Reload();
            }

            DrawTree();

            string selectedAsset = "";
            if (treeView != null && treeView.lastSelected?.target)
            {
                selectedAsset = treeView.lastSelected.assetPath.Substring("Assets/".Length);
            }
            GUILayout.Label(selectedAsset);
        }

        void DrawTree()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
            {
                Reload();
            }


            this.rootPath = GUILayout.TextField(this.rootPath, EditorStyles.toolbarTextField, GUILayout.Width(position.width * 0.4f));
            if (this.rootPath != rootTreeView.rootPath)
            {
                EditorUserSettings.SetConfigValue("Moths/ScriptableObjectBrowser/rootPath", rootTreeView.rootPath = rootPath);
                Reload();
            }

            GUILayout.FlexibleSpace();

            searchString = searchField.OnToolbarGUI(searchString);
            if (treeView.searchFilter != searchString)
            {
                treeView.searchFilter = searchString;
                rootTreeView.searchFilter = searchString;

                Reload();
            }

            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();

            var rect = GUILayoutUtility.GetRect(0, 100000, 0, 100000);

            var rootViewRect = new Rect(0, rect.y, position.width * 0.35f, rect.height);
            var treeViewRect = new Rect(rootViewRect.width, rootViewRect.y, position.width - rootViewRect.width, rootViewRect.height);

            if (treeView.rootPath != rootTreeView.selected)
            {
                treeView.rootPath = rootTreeView.selected;
                treeView.Reload();
            }
            treeView.OnGUI(treeViewRect);

            rootTreeView.OnGUI(rootViewRect);

            EditorGUILayout.EndHorizontal();
        }

        internal void Reload()
        {
            treeView.Reload();
            rootTreeView.Reload();
        }

        internal static string GetItemPath(string rootPath, TreeViewItem item)
        {
            List<string> parts = new List<string>();

            TreeViewItem current = item;
            while (current != null && current.depth >= 0) // skip root (-1 depth)
            {
                parts.Add(current.displayName);
                current = current.parent;
            }

            parts.Reverse();

            // Root of project assets
            return Path.Combine("Assets", rootPath, string.Join("/", parts)).Replace('\\', '/');
        }

    }

    class FolderItem : TreeViewItem
    {
        public FolderItem() : base() { }
    }
}