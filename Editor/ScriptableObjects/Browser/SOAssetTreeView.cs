using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using System.IO;

using Object = UnityEngine.Object;

namespace Moths.ScriptableObjects.Browser
{
    class SOAssetTreeView : TreeView
    {
        private ScriptableObjectBrowser _browser;

        public bool isFavourites;
        TypeEntry[] types;

        public SOItem selectDragItem;
        public SOItem lastSelected;
        public string rootPath = "";
        public SearchFilter searchFilter;

        private Dictionary<string, List<SOItem>> IdLookup = new Dictionary<string, List<SOItem>>();

        public SOAssetTreeView(ScriptableObjectBrowser browser, TreeViewState state, TypeEntry[] types) : base(state)
        {
            _browser = browser;
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

            IdLookup.Clear();

            foreach (var guid in guids)
            {
                if (isFavourites && !_browser.favourites.Contains(guid)) continue;

                string path = AssetDatabase.GUIDToAssetPath(guid);

                if (!path.StartsWith(rootPath)) continue;

                path = path.Substring(rootPath.Length);

                string assetName = Path.GetFileNameWithoutExtension(path);


                ScriptableObject mainSO = AssetDatabase.LoadMainAssetAtPath(rootPath + path) as ScriptableObject;
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


                // Skip files without any SOs
                if (mainSO == null) continue;

                // Create parent item (the asset itself)
                var mainItem = new SOItem
                {
                    id = ++id,
                    depth = parts.Length - 1,
                    displayName = mainSO.name,
                    guid = guid,
                    isFavourite = _browser.favourites.Contains(guid),
                    target = mainSO
                };
                if (!IdLookup.TryGetValue(guid, out var list)) list = IdLookup[guid] = new List<SOItem>();
                IdLookup[guid].Add(mainItem);

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
                            isFavourite = _browser.favourites.Contains(guid),
                            target = (ScriptableObject)so,
                            isSubAsset = true,
                        };
                        IdLookup[guid].Add(subItem);

                        mainItem.AddChild(subItem);
                    }
                }
            }

            if (!root.hasChildren)
                root.AddChild(new TreeViewItem { id = ++id, depth = 0, displayName = "No ScriptableObjects found" });

            SetupDepthsFromParentsAndChildren(root);
            return root;
        }

        protected override bool CanStartDrag(CanStartDragArgs args) => true;

        protected override void SetupDragAndDrop(SetupDragAndDropArgs args)
        {
            if (selectDragItem != null)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = new Object[] { selectDragItem.target };
                DragAndDrop.StartDrag(selectDragItem.target.name);
            }
        }

        protected override void RowGUI(RowGUIArgs args)
        {
            var item = args.item as SOItem;
            if (item != null)
            {
                var obj = item.target;

                if (args.selected)
                {
                    selectDragItem = item;
                    if (Event.current.type == EventType.MouseUp)
                    {
                        if (lastSelected != item)
                        {
                            Selection.activeObject = obj;
                            lastSelected = item;
                        }
                    }
                }

                Rect rect = args.rowRect;
                rect.width -= 20;

                Rect selectRect = new Rect(14 + 14 * args.item.depth, rect.y, 20, rect.height);
                if (GUI.Button(selectRect, AssetPreview.GetMiniThumbnail(obj)))
                {
                    AssetDatabase.OpenAsset(obj);
                }

                args.rowRect = rect;
                args.label = "      " + args.label;
                base.RowGUI(args);


                if (item.isSubAsset) return;

                Rect favRect = new Rect(rect.xMax - 20, rect.y, 20, rect.height);
                if (GUI.Button(favRect, item.isFavourite ? "★" : "☆", EditorStyles.label))
                {
                    item.isFavourite = !item.isFavourite;
                    _browser.ToggleFavourite(item.guid, item.isFavourite);
                    if (isFavourites) _browser.Reload();
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

            string itemPath = ScriptableObjectBrowser.GetItemPath(rootPath, item);

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
                menu.AddSeparator("");

                menu.AddItem(new GUIContent("Open"), false, () =>
                {
                    AssetDatabase.OpenAsset(soItem.target);
                });

                menu.AddItem(new GUIContent("Duplicate"), false, () =>
                {
                    SOEditorUtility.Duplicate(soItem.target);
                    Reload();
                });

                menu.AddSeparator("");

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
                            _browser.ToggleFavourite(soItem.guid, false);
                        });
                    }
                    else
                    {
                        menu.AddItem(new GUIContent("Add to Favourites"), false, () =>
                        {
                            soItem.isFavourite = true;
                            _browser.ToggleFavourite(soItem.guid, true);
                        });
                    }
                }

                menu.ShowAsContext();
            }
            else
            {
                GenericMenu menu = new GenericMenu();

                menu.AddItem(new GUIContent("Create/Folder"), false, () =>
                {
                    string folderGuid = AssetDatabase.CreateFolder(itemPath, "New Folder");
                    SOEditorUtility.CreateToFolder(typeof(ScriptableObjectContainer), AssetDatabase.GUIDToAssetPath(folderGuid));
                    Reload();
                });

                menu.AddSeparator("Create");

                foreach (var type in types)
                {
                    menu.AddItem(new GUIContent("Create/" + type.createPath), false, () =>
                    {
                        SOEditorUtility.CreateToFolder(type.type, itemPath);
                        Reload();
                    });
                }

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("Delete"), false, () =>
                {
                    AssetDatabase.DeleteAsset(itemPath);
                    Reload();
                });

                menu.AddItem(new GUIContent("Rename"), false, () =>
                {
                    TextPromptWindow.Show("Rename", item.displayName, newName =>
                    {
                        AssetDatabase.RenameAsset(itemPath, newName);
                        Reload();
                    });
                });

                menu.ShowAsContext();
            }
        }

        public int FindIdByGuid(string guid, Object obj)
        {
            if (IdLookup.TryGetValue(guid, out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].target == obj) return list[i].id;
                }
                return -1;
            }
            return -1;
        }

    }
}