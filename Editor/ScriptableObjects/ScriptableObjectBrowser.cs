using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.IO;

using Object = UnityEngine.Object;
using System.Linq;

namespace Moths.ScriptableObjects
{
    public class ScriptableObjectBrowser : EditorWindow
    {
        [MenuItem("Window/ScriptableObject Browser")]
        public static void Open()
        {
            var wnd = GetWindow<ScriptableObjectBrowser>();
            wnd.titleContent = new GUIContent("SO Browser");
            wnd.Show();
        }

        TreeViewState treeViewState;
        SOAssetTreeView treeView;
        Vector2 favouritesScroll;
        SearchField searchField;

        string rootPath;

        string searchString = "";

        int selectedTab = 0; // 0 = All, 1 = Favourites

        void OnEnable()
        {
            if (treeViewState == null)
                treeViewState = new TreeViewState();

            treeView = new SOAssetTreeView(treeViewState, SOEditorUtility.Types.ToArray());
            treeView.searchString = "";
            treeView.Reload();

            rootPath = EditorPrefs.GetString("Moths/ScriptableObjectBrowser/rootPath");

            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += treeView.SetFocusAndEnsureSelectedItem;
        }

        void OnGUI()
        {
            selectedTab = GUILayout.Toolbar(selectedTab, new[] { "All", "Favourites" });

            if (selectedTab == 0)
            {
                DrawAllTab();
            }
            else if (selectedTab == 1)
            {
                DrawFavouritesTab();
            }

            string selectedAsset = "";
            if (treeView != null && treeView.lastSelected?.target)
            {
                selectedAsset = treeView.lastSelected.assetPath.Substring("Assets/".Length);
            }
            GUILayout.Label(selectedAsset);
        }

        void DrawAllTab()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton))
                treeView.Reload();

            
            this.rootPath = GUILayout.TextField(this.rootPath, EditorStyles.toolbarTextField, GUILayout.Width(position.width * 0.4f));
            if (this.rootPath != treeView.rootPath)
            {
                EditorPrefs.SetString("Moths/ScriptableObjectBrowser/rootPath", treeView.rootPath = rootPath);
                treeView.Reload();
            }

            GUILayout.FlexibleSpace();

            searchString = searchField.OnToolbarGUI(searchString);
            if (treeView.searchFilter != searchString)
            {
                treeView.searchFilter = searchString;
                treeView.Reload();
            }

            EditorGUILayout.EndHorizontal();

            treeView.OnGUI(GUILayoutUtility.GetRect(0, 100000, 0, 100000));
        }

        internal static bool DrawRow(string guid, ScriptableObject obj, bool isFavourite)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button(AssetPreview.GetMiniThumbnail(obj), GUILayout.Width(20), GUILayout.Height(20)))
            {
                AssetDatabase.OpenAsset(obj);
            }

            if (GUILayout.Button(obj.name, EditorStyles.label))
            {
                Selection.activeObject = obj;
            }

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(isFavourite ? "★" : "☆", EditorStyles.label))
            {
                SOAssetTreeView.RemoveFavourite(guid);
                return false; // refresh next frame
            }

            EditorGUILayout.EndHorizontal();

            return true;
        }

        Dictionary<string, bool> expandedFavourites = new Dictionary<string, bool>();

        void DrawFavouritesTab()
        {
            var favourites = SOAssetTreeView.LoadFavourites();

            favouritesScroll = EditorGUILayout.BeginScrollView(favouritesScroll);
            foreach (var guid in favourites)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (string.IsNullOrEmpty(path)) continue;

                ScriptableObject mainSO = AssetDatabase.LoadMainAssetAtPath(path) as ScriptableObject;
                if (mainSO == null) continue;

                Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

                // Ensure we track this guid in expanded state
                if (!expandedFavourites.ContainsKey(guid))
                    expandedFavourites[guid] = false;

                // --- Main asset row
                EditorGUILayout.BeginHorizontal();

                if (subAssets != null && subAssets.Length > 0)
                {
                    // Draw arrow toggle
                    string arrow = expandedFavourites[guid] ? "▼" : "▶";
                    Rect rect = GUILayoutUtility.GetRect(20, EditorGUIUtility.singleLineHeight);
                    expandedFavourites[guid] = EditorGUI.Foldout(rect, expandedFavourites[guid], GUIContent.none, false);
                }
                else
                {
                    EditorGUILayout.Space(20);
                }

                // Draw main asset row content
                if (!DrawRow(guid, mainSO, true))
                {
                    EditorGUILayout.EndHorizontal();
                    break;
                }

                EditorGUILayout.EndHorizontal();

                // --- Sub-assets (only if expanded)
                if (expandedFavourites[guid])
                {
                    if (subAssets != null && subAssets.Length > 0)
                    {
                        foreach (var so in subAssets)
                        {
                            EditorGUILayout.BeginHorizontal();
                            GUILayout.Space(50); // indent under arrow + icon

                            if (GUILayout.Button(AssetPreview.GetMiniThumbnail(so), GUILayout.Width(20), GUILayout.Height(20)))
                            {
                                AssetDatabase.OpenAsset(so);
                            }

                            if (GUILayout.Button(so.name, EditorStyles.label))
                            {
                                Selection.activeObject = so;
                            }

                            EditorGUILayout.EndHorizontal();
                        }
                    }
                }
            }
            EditorGUILayout.EndScrollView();
        }
    }

    class SOAssetTreeView : TreeView
    {
        HashSet<string> favourites;
        TypeEntry[] types;

        public SOItem lastSelected;
        public string rootPath = "";
        public string searchFilter;

        public SOAssetTreeView(TreeViewState state, TypeEntry[] types) : base(state)
        {
            favourites = LoadFavourites();
            this.types = types;
        }

        protected override TreeViewItem BuildRoot()
        {
            string rootPath = "Assets/" + this.rootPath;
            rootPath = rootPath.TrimEnd('/') + '/';

            var root = new TreeViewItem { id = 0, depth = -1, displayName = "Root" };

            string[] guids = AssetDatabase.FindAssets("t:ScriptableObject", new[] { "Assets" });

            Dictionary<string, FolderItem> folderCache = new Dictionary<string, FolderItem>();
            int id = 1;

            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!path.StartsWith(rootPath)) continue;

                path = path.Substring(rootPath.Length);

                string assetName = Path.GetFileNameWithoutExtension(path);

                //Apply search filter
                if (!string.IsNullOrEmpty(searchFilter) &&
                    !path.ToLower().Contains(searchFilter.ToLower()))
                    continue;

                string[] parts = path.Split('/');
                FolderItem parent = null;
                string currentPath = "";

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    currentPath = (currentPath == "") ? parts[i] : currentPath + "/" + parts[i];

                    if (!folderCache.TryGetValue(currentPath, out var folder))
                    {
                        folder = new FolderItem { id = ++id, depth = i, displayName = parts[i] };
                        folderCache[currentPath] = folder;

                        if (parent != null)
                            parent.AddChild(folder);
                        else
                            root.AddChild(folder);
                    }

                    parent = folder;
                }

                Object[] objs = AssetDatabase.LoadAllAssetsAtPath(rootPath + path);
                ScriptableObject mainSO = AssetDatabase.LoadMainAssetAtPath(rootPath + path) as ScriptableObject;
                Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(rootPath + path);

                // Skip files without any SOs
                if (mainSO == null) continue;

                // Create parent item (the asset itself)
                var mainItem = new SOItem
                {
                    id = ++id,
                    depth = parts.Length - 1,
                    displayName = mainSO.name,
                    guid = guid,
                    isFavourite = favourites.Contains(guid),
                    target = mainSO
                };

                // Attach to folder
                if (parent != null)
                    parent.AddChild(mainItem);
                else
                    root.AddChild(mainItem);

                // Add sub-assets as children
                foreach (var so in subAssets)
                {
                    if (so is ScriptableObject)
                    {
                        var subItem = new SOItem
                        {
                            id = ++id,
                            depth = parts.Length, // one deeper than parent
                            displayName = so.name,
                            guid = guid,          // same file GUID
                            isFavourite = favourites.Contains(guid),
                            target = (ScriptableObject)so,
                            isSubAsset = true,
                        };
                        mainItem.AddChild(subItem);
                    }
                }
            }

            if (!root.hasChildren)
                root.AddChild(new TreeViewItem { id = ++id, depth = 0, displayName = "No ScriptableObjects found" });

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }


        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as SOItem;
            if (item != null)
            {
                var obj = item.target;

                if (args.selected)
                {
                    if (lastSelected != item)
                    {
                        Selection.activeObject = obj;
                        lastSelected = item;
                    }
                }

                Rect rect = args.rowRect;
                rect.width -= 20;

                Rect selectRect = new Rect(14 + 14 * args.item.depth, rect.y, 20, rect.height);
                if (GUI.Button(selectRect, AssetPreview.GetMiniThumbnail(obj)))
                {
                    Selection.activeObject = obj;
                }

                args.rowRect = rect;
                args.label = "      " + args.label;
                base.RowGUI(args);


                if (item.isSubAsset) return;

                Rect favRect = new Rect(rect.xMax - 20, rect.y, 20, rect.height);
                if (GUI.Button(favRect, item.isFavourite ? "★" : "☆", EditorStyles.label))
                {
                    item.isFavourite = !item.isFavourite;
                    ToggleFavourite(item.guid, item.isFavourite);
                }
            }
            else
            {
                args.label = "📁 " + args.label;
                base.RowGUI(args);
            }
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            if (item == null) return;

            string itemPath = GetItemPath(item);

            if (item is SOItem)
            {
                var soItem = (SOItem)item;
                itemPath = AssetDatabase.GUIDToAssetPath(soItem.guid);

                GenericMenu menu = new GenericMenu();

                foreach (var type in types)
                {
                    menu.AddItem(new GUIContent("Create/" + type.createPath), false, () =>
                    {
                        SOEditorUtility.CreateToAsset(type.type, AssetDatabase.LoadMainAssetAtPath(itemPath));
                        Reload();
                    });
                }

                menu.AddItem(new GUIContent("Open"), false, () =>
                {
                    AssetDatabase.OpenAsset(soItem.target);
                });

                menu.AddItem(new GUIContent("Duplicate"), false, () =>
                {
                    SOEditorUtility.Duplicate(soItem.target);
                    Reload();
                });

                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    SOEditorUtility.Delete(soItem.target);
                    Reload();
                });

                menu.AddItem(new GUIContent("Rename"), false, () =>
                {
                    var target = soItem.target;
                    if (target == null) return;
                    SOEditorUtility.Rename(target, Reload);
                });

                if (!soItem.isSubAsset)
                {
                    menu.AddSeparator("");

                    // Toggle favourite
                    if (soItem.isFavourite)
                    {
                        menu.AddItem(new GUIContent("Remove from Favourites"), false, () =>
                        {
                            soItem.isFavourite = false;
                            ToggleFavourite(soItem.guid, false);
                        });
                    }
                    else
                    {
                        menu.AddItem(new GUIContent("Add to Favourites"), false, () =>
                        {
                            soItem.isFavourite = true;
                            ToggleFavourite(soItem.guid, true);
                        });
                    }
                }

                menu.ShowAsContext();
            }
            else
            {
                GenericMenu menu = new GenericMenu();

                foreach (var type in types)
                {
                    menu.AddItem(new GUIContent("Create/" + type.createPath), false, () =>
                    {
                        SOEditorUtility.CreateToFolder(type.type, itemPath);
                        Reload();
                    });
                }



                menu.ShowAsContext();
            }


        }


        private static string GetItemPath(TreeViewItem item)
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
            return "Assets/" + string.Join("/", parts);
        }

        void ToggleFavourite(string guid, bool fav)
        {
            if (fav) favourites.Add(guid);
            else favourites.Remove(guid);
            SaveFavourites();
        }

        public static HashSet<string> LoadFavourites()
        {
            var json = EditorPrefs.GetString("Moths/ScriptableObjectBrowser/favourites", "{\"items\":[]}");
            return new HashSet<string>(JsonUtility.FromJson<StringArray>(json).items);
        }

        public static void RemoveFavourite(string guid)
        {
            var favs = LoadFavourites();
            favs.Remove(guid);
            var arr = new StringArray { items = new List<string>(favs).ToArray() };
            EditorPrefs.SetString("Moths/ScriptableObjectBrowser/favourites", JsonUtility.ToJson(arr));
        }

        void SaveFavourites()
        {
            var arr = new StringArray { items = new List<string>(favourites).ToArray() };
            EditorPrefs.SetString("Moths/ScriptableObjectBrowser/favourites", JsonUtility.ToJson(arr));
        }

        [System.Serializable]
        class StringArray { public string[] items; }
    }

    class SOItem : TreeViewItem
    {
        private ScriptableObject _target;

        public string guid;
        public bool isFavourite;
        public bool isSubAsset;
        public ScriptableObject target
        {
            get => _target;
            set
            {
                _target = value;
                assetPath = AssetDatabase.GetAssetPath(value);
            }
        }

        public string assetPath;
    }

    class FolderItem : TreeViewItem
    {
        public FolderItem() : base() { }
    }
}