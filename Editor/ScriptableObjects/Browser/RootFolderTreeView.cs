using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.IO;

namespace Moths.ScriptableObjects.Browser
{
    class RootFolderTreeView : TreeView
    {
        private ScriptableObjectBrowser _browser;

        public string rootPath;
        public SearchFilter searchFilter;
        public bool isFavourites;

        public string selected;

        public RootFolderTreeView(ScriptableObjectBrowser browser, TreeViewState state) : base(state)
        {
            _browser = browser;
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
                if (isFavourites && !_browser.favourites.Contains(guid)) continue;

                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!path.StartsWith(rootPath)) continue;

                path = path.Substring(rootPath.Length);

                string assetName = Path.GetFileNameWithoutExtension(path);

                Object[] subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(rootPath + path);

                if (!searchFilter.Apply(path.ToLower()))
                {
                    for (int i = 0; i < subAssets.Length; i++)
                    {
                        if (searchFilter.Apply(subAssets[i].name)) goto SKIP;
                    }
                    continue;

                SKIP:;
                }

                string[] parts = path.Split('/');

                string currentPath = parts[0];

                if (folderCache.TryGetValue(currentPath, out var folder)) continue;

                folder = new FolderItem { id = ++id, depth = 0, displayName = parts[0] };
                folderCache[currentPath] = folder;

                root.AddChild(folder);
            }

            if (!root.hasChildren)
                root.AddChild(new TreeViewItem { id = ++id, depth = 0, displayName = "No ScriptableObjects found" });

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            args.label = "📁 " + args.label;
            base.RowGUI(args);

            if (args.selected)
            {
                selected = Path.Combine(rootPath, args.item.displayName).Replace('\\', '/');
            }
        }

        protected override bool CanStartDrag(CanStartDragArgs args) => false;
        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
        }

        protected override void ContextClickedItem(int id)
        {
            var item = FindItem(id, rootItem);
            if (item == null) return;

            string itemPath = ScriptableObjectBrowser.GetItemPath(rootPath, item);

            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("Create/Folder"), false, () =>
            {
                string folderGuid = AssetDatabase.CreateFolder(itemPath, "New Folder");
                SOEditorUtility.CreateToFolder(typeof(ScriptableObjectContainer), AssetDatabase.GUIDToAssetPath(folderGuid));
                _browser.Reload();
            });

            menu.AddSeparator("Create");

            foreach (var type in SOEditorUtility.Types)
            {
                menu.AddItem(new GUIContent("Create/" + type.createPath), false, () =>
                {
                    SOEditorUtility.CreateToFolder(type.type, itemPath);
                    _browser.Reload();
                });
            }

            menu.AddSeparator("");

            menu.AddItem(new GUIContent("Delete"), false, () =>
            {
                AssetDatabase.DeleteAsset(itemPath);
                _browser.Reload();
            });

            menu.AddItem(new GUIContent("Rename"), false, () =>
            {
                TextPromptWindow.Show("Rename", item.displayName, newName =>
                {
                    AssetDatabase.RenameAsset(itemPath, newName);
                    _browser.Reload();
                });
            });

            menu.ShowAsContext();
        }
    }
}