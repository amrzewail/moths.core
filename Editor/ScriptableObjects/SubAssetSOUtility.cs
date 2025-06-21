using UnityEditor;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Object = UnityEngine.Object;
using System.IO;

namespace Moths.Editor.ScriptableObjects
{
    public static class SubAssetSOUtility
    {
        public static void ShowTypePicker(ScriptableObject container)
        {
            var scriptableObjectTypes = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    t.IsSubclassOf(typeof(ScriptableObject)) &&
                    !t.IsAbstract &&
                    t.GetCustomAttributes(typeof(CreateAssetMenuAttribute), false).Length > 0)
                .ToList();

            if (scriptableObjectTypes.Count == 0)
            {
                EditorUtility.DisplayDialog("No Types", "No ScriptableObject types with [CreateAssetMenu] found.", "OK");
                return;
            }

            var window = EditorWindow.GetWindow<TypeSelectionPopup>(true, "Select ScriptableObject Type");
            window.Initialize(container, scriptableObjectTypes);
            window.ShowUtility();
        }

        [MenuItem("Assets/ScriptableObjects/Create", true)]
        private static bool ValidateCreate()
        {
            return Selection.activeObject is ScriptableObject;
        }

        [MenuItem("Assets/ScriptableObjects/Create")]
        private static void Create()
        {
            var target = Selection.activeObject as ScriptableObject;
            ShowTypePicker(target);
        }

        [MenuItem("Assets/ScriptableObjects/Rename", true)]
        private static bool ValidateRename()
        {
            return Selection.activeObject is ScriptableObject;
        }

        [MenuItem("Assets/ScriptableObjects/Rename")]
        private static void RenameCustomName()
        {
            var target = Selection.activeObject as ScriptableObject;

            if (target == null) return;

            TextPromptWindow.Show("Rename", target.name, newName =>
            {
                Undo.RecordObject(target, "Rename");
                target.name = newName;
                EditorUtility.SetDirty(target);
                AssetDatabase.SaveAssets();
            });
        }

        [MenuItem("Assets/ScriptableObjects/Duplicate", true)]
        private static bool ValidateDuplicate()
        {
            return Selection.activeObject is ScriptableObject;
        }

        [MenuItem("Assets/ScriptableObjects/Duplicate")]
        private static void DuplicateSelected()
        {
            var original = Selection.activeObject as ScriptableObject;
            if (original == null) return;

            string path = AssetDatabase.GetAssetPath(original);
            bool isSubAsset = AssetDatabase.IsSubAsset(original);

            if (isSubAsset)
            {
                // Duplicate sub-asset
                ScriptableObject clone = Object.Instantiate(original);
                clone.name = original.name;

                Undo.RegisterCreatedObjectUndo(clone, "Duplicate Sub-Asset");
                AssetDatabase.AddObjectToAsset(clone, AssetDatabase.LoadMainAssetAtPath(path));
                AssetDatabase.ImportAsset(path);
                EditorUtility.SetDirty(clone);

                Debug.Log($"Duplicated sub-asset '{original.name}' inside '{path}'");
            }
            else
            {
                // Duplicate standalone asset
                string directory = Path.GetDirectoryName(path);
                string filename = Path.GetFileNameWithoutExtension(path);
                string extension = Path.GetExtension(path);

                string newPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(directory, filename + " Copy" + extension));
                AssetDatabase.CopyAsset(path, newPath);
                AssetDatabase.Refresh();

                Debug.Log($"Duplicated asset to '{newPath}'");
            }
        }

        [MenuItem("Assets/ScriptableObjects/Delete", true)]
        private static bool ValidateDelete()
        {
            return Selection.activeObject is ScriptableObject;
        }

        [MenuItem("Assets/ScriptableObjects/Delete")]
        private static void DeleteSelected()
        {
            var target = Selection.activeObject as ScriptableObject;
            if (target == null) return;

            string path = AssetDatabase.GetAssetPath(target);

            // Check if it's a sub-asset (has same path as container)
            if (!string.IsNullOrEmpty(path) && AssetDatabase.IsSubAsset(target))
            {
                if (!EditorUtility.DisplayDialog(
                        "Delete Sub-Asset",
                        $"Are you sure you want to delete '{target.name}' from asset '{path}'?",
                        "Delete", "Cancel"))
                    return;

                Undo.RegisterCompleteObjectUndo(AssetDatabase.LoadMainAssetAtPath(path), "Delete Sub-Asset");
                Object.DestroyImmediate(target, true);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(path);
            }
            else if (!string.IsNullOrEmpty(path))
            {
                if (!EditorUtility.DisplayDialog(
                        "Delete Asset",
                        $"Are you sure you want to delete the entire asset '{path}'?",
                        "Delete", "Cancel"))
                    return;

                AssetDatabase.DeleteAsset(path);
            }
            else
            {
                Debug.LogWarning("Cannot delete: target is not a valid asset.");
            }
        }


    }

    public class TextPromptWindow : EditorWindow
    {
        private string inputText = "";
        private Action<string> onSubmit;
        private bool submitted = false;

        public static void Show(string title, string text, Action<string> onSubmit)
        {
            var window = CreateInstance<TextPromptWindow>();
            window.titleContent = new GUIContent(title);
            window.onSubmit = onSubmit;
            window.position = new Rect(Screen.width / 2, Screen.height / 2, 300, 80);
            window.inputText = text;
            window.ShowModal(); // BLOCKING call
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Enter Text:");
            GUI.SetNextControlName("TextField");
            inputText = EditorGUILayout.TextField(inputText);

            GUILayout.Space(10);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK"))
            {
                submitted = true;
                onSubmit?.Invoke(inputText);
                Close();
            }
            if (GUILayout.Button("Cancel"))
            {
                submitted = false;
                Close();
            }
            GUILayout.EndHorizontal();

            if (!submitted)
                EditorGUI.FocusTextInControl("TextField");
        }
    }

    public class TypeSelectionPopup : EditorWindow
    {
        private ScriptableObject _container;
        private string _search = "";
        private Vector2 _scroll;
        private List<Type> _allTypes;

        private TreeNode _root = new TreeNode("ROOT");

        private class TreeNode
        {
            public string Name;
            public Dictionary<string, TreeNode> Children = new();
            public List<TypeEntry> Types = new();
            public bool Expanded = false;
            public TreeNode(string name) => Name = name;
        }

        private struct TypeEntry
        {
            public Type Type;
            public string DisplayName;
        }

        public void Initialize(ScriptableObject container, List<Type> types)
        {
            _allTypes = types;
            _container = container;
            BuildTree(types, false);
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Select ScriptableObject Type", EditorStyles.boldLabel);
            EditorGUI.BeginChangeCheck();
            _search = EditorGUILayout.TextField("Search", _search);
            if (EditorGUI.EndChangeCheck())
            {
                _root = new TreeNode("ROOT");
                BuildTree(FilterList(), !string.IsNullOrEmpty(_search));
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawTree(_root, 0);
            EditorGUILayout.EndScrollView();
        }

        private List<Type> FilterList()
        {
            string lower = _search.ToLowerInvariant();
            return string.IsNullOrWhiteSpace(lower)
                ? _allTypes
                : _allTypes.Where(t => GetMenuName(t).ToLowerInvariant().Contains(lower)).ToList();
        }

        private void BuildTree(List<Type> types, bool flatten)
        {
            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<CreateAssetMenuAttribute>();
                if (attr == null || string.IsNullOrEmpty(attr.menuName))
                    continue;

                var pathParts = attr.menuName.Split('/');
                TreeNode current = _root;

                if (!flatten)
                {
                    for (int i = 0; i < pathParts.Length - 1; i++)
                    {
                        if (!current.Children.TryGetValue(pathParts[i], out var child))
                        {
                            child = new TreeNode(pathParts[i]);
                            current.Children[pathParts[i]] = child;
                        }
                        current = child;
                    }
                }

                current.Types.Add(new TypeEntry
                {
                    Type = type,
                    DisplayName = flatten ? attr.menuName : pathParts.Last()
                });
            }
        }

        private void DrawTree(TreeNode node, int indent)
        {
            foreach (var child in node.Children.Values.OrderBy(c => c.Name))
            {
                EditorGUI.indentLevel = indent;
                child.Expanded = EditorGUILayout.Foldout(child.Expanded, child.Name, true);
                if (child.Expanded)
                {
                    DrawTree(child, indent + 1);
                }
            }

            foreach (var entry in node.Types
                .Where(t => string.IsNullOrWhiteSpace(_search) || t.DisplayName.ToLower().Contains(_search.ToLower()))
                .OrderBy(t => t.DisplayName))
            {
                EditorGUI.indentLevel = indent;
                if (GUILayout.Button(entry.DisplayName, EditorStyles.miniButton))
                {
                    CreateAndAdd(entry.Type);
                    Close();
                }
            }
        }

        private string GetMenuName(Type type)
        {
            var attr = type.GetCustomAttribute<CreateAssetMenuAttribute>();
            if (attr != null && !string.IsNullOrEmpty(attr.menuName))
                return attr.menuName;

            return type.Name;
        }

        private void CreateAndAdd(Type type)
        {
            var child = ScriptableObject.CreateInstance(type);

            var attr = type.GetCustomAttribute<CreateAssetMenuAttribute>();
            if (attr == null || string.IsNullOrEmpty(attr.fileName))
            {
                child.name = type.Name;
            }
            else
            {
                child.name = attr.fileName;
            }

            AssetDatabase.AddObjectToAsset(child, _container);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_container));
            AssetDatabase.SaveAssets();

            EditorUtility.SetDirty(_container);
            Debug.Log($"Added {type.Name} to {_container.name}.");
        }

    }
}