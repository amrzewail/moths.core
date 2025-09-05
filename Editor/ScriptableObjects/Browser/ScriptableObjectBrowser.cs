using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using Object = UnityEngine.Object;
using System.Reflection;
using UnityEditorInternal;

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

        internal HashSet<string> favourites;

        string rootPath;
        string searchString = "";
        int selectedTab = 0; // 0 = All, 1 = Favourites

        Object pingObject;
        bool skipNextSelection;

        void OnEnable()
        {
            favourites = LoadFavourites();

            var treeViewStateJson = EditorUserSettings.GetConfigValue("Moths/ScriptableObjectBrowser/treeViewState");
            var rootTreeViewStateJson = EditorUserSettings.GetConfigValue("Moths/ScriptableObjectBrowser/rootViewState");
            rootPath = EditorUserSettings.GetConfigValue("Moths/ScriptableObjectBrowser/rootPath");
            
            treeViewState = string.IsNullOrEmpty(treeViewStateJson) ? new TreeViewState() : JsonUtility.FromJson<TreeViewState>(treeViewStateJson);
            rootViewState = string.IsNullOrEmpty(rootTreeViewStateJson) ? new TreeViewState() : JsonUtility.FromJson<TreeViewState>(rootTreeViewStateJson);

            treeView = new SOAssetTreeView(this, treeViewState, SOEditorUtility.Types.ToArray());
            treeView.searchString = "";

            rootTreeView = new RootFolderTreeView(this, rootViewState);
            rootTreeView.searchString = "";
            
            Reload();

            if (string.IsNullOrEmpty(rootPath)) rootPath = "";

            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;

            Selection.selectionChanged += SelectionChangedCallback;
        }

        private void OnDisable()
        {
            EditorUserSettings.SetConfigValue("Moths/ScriptableObjectBrowser/treeViewState", JsonUtility.ToJson(treeViewState));
            EditorUserSettings.SetConfigValue("Moths/ScriptableObjectBrowser/rootViewState", JsonUtility.ToJson(rootViewState));
            EditorUserSettings.SetConfigValue("Moths/ScriptableObjectBrowser/rootPath", rootPath ?? "");
         
            Selection.selectionChanged -= SelectionChangedCallback;
        }

        private void OnGUI()
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

            if (pingObject)
            {
                //PingByGuid(rootViewRect, treeViewRect);
            }
            else
            {

            }

            treeView.OnGUI(treeViewRect);
            rootTreeView.OnGUI(rootViewRect);

            PingByGuid(rootViewRect, treeViewRect);

            EditorGUILayout.EndHorizontal();
        }

        private void PingByGuid(Rect rootViewRect, Rect treeViewRect)
        {
            if (!pingObject) return;

            string pingGuid = AssetDatabase.GUIDFromAssetPath(AssetDatabase.GetAssetPath(Selection.activeObject)).ToString();

            int rootId = rootTreeView.FindIdByGuid(pingGuid);
            if (rootId == -1) goto EXIT;

            rootTreeView.SetSelection(new[] { rootId });
            rootTreeView.FrameItem(rootId);
            rootTreeView.OnGUI(rootViewRect);
            rootTreeView.UpdateSelected(rootId);

            if (treeView.rootPath != rootTreeView.selected)
            {
                treeView.rootPath = rootTreeView.selected;
                treeView.Reload();
            }

            int id = treeView.FindIdByGuid(pingGuid, pingObject);
            if (id == -1) goto EXIT;

            treeView.SetSelection(new[] { id });
            treeView.FrameItem(id);
            treeView.OnGUI(treeViewRect);

        EXIT:
            if (treeView?.lastSelected?.target != null)
            {
                Selection.activeObject = treeView.lastSelected.target;
                skipNextSelection = true;
            }
            pingObject = null;
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

        private void SelectionChangedCallback()
        {
            if (skipNextSelection)
            {
                skipNextSelection = false;
                return;
            }
            if (Selection.activeObject is not ScriptableObject) return;

            pingObject = Selection.activeObject;

            EditorApplication.QueuePlayerLoopUpdate();
            Repaint();
        }

        private static HashSet<string> LoadFavourites()
        {
            var json = EditorUserSettings.GetConfigValue("Moths/ScriptableObjectBrowser/favourites");
            if (string.IsNullOrEmpty(json)) json = "{\"items\":[]}";
            return new HashSet<string>(JsonUtility.FromJson<StringArray>(json).items);
        }

        public static void RemoveFavourite(string guid)
        {
            var favs = LoadFavourites();
            favs.Remove(guid);
            var arr = new StringArray { items = new List<string>(favs).ToArray() };
            EditorUserSettings.SetConfigValue("Moths/ScriptableObjectBrowser/favourites", JsonUtility.ToJson(arr));
        }

        internal void SaveFavourites()
        {
            var arr = new StringArray { items = new List<string>(favourites).ToArray() };
            EditorUserSettings.SetConfigValue("Moths/ScriptableObjectBrowser/favourites", JsonUtility.ToJson(arr));
        }

        internal void ToggleFavourite(string guid, bool fav)
        {
            if (fav) favourites.Add(guid);
            else favourites.Remove(guid);
            SaveFavourites();
        }

        [System.Serializable]
        class StringArray { public string[] items; }

    }

    class FolderItem : TreeViewItem
    {
        public FolderItem() : base() { }
    }
}