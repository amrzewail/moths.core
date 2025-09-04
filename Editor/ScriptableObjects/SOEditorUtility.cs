using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Moths.ScriptableObjects
{
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

    public struct TypeEntry
    {
        public Type type;
        public string displayName;
        public string createPath;
    }

    [InitializeOnLoad]
    public static class SOEditorUtility
    {
        private static List<TypeEntry> _types = new List<TypeEntry>();

        public static IReadOnlyList<TypeEntry> Types => _types;

        static SOEditorUtility()
        {
            RefreshTypes();
        }

        public static void RefreshTypes()
        {
            var types = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t =>
                    t.IsSubclassOf(typeof(ScriptableObject)) &&
                    !t.IsAbstract &&
                    t.GetCustomAttributes(typeof(CreateAssetMenuAttribute), false).Length > 0)
                .ToList();

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<CreateAssetMenuAttribute>();
                if (attr == null || string.IsNullOrEmpty(attr.menuName))
                    continue;

                var pathParts = attr.menuName.Split('/');

                _types.Add(new TypeEntry
                {
                    type = type,
                    createPath = attr.menuName,
                    displayName = pathParts.Last()
                });
            }

            _types.Sort((l1, l2) => l1.createPath.CompareTo(l2.createPath));
        }

        public static ScriptableObject CreateToAsset(Type type, Object mainAsset)
        {
            var obj = ScriptableObject.CreateInstance(type);

            var attr = type.GetCustomAttribute<CreateAssetMenuAttribute>();
            if (attr == null || string.IsNullOrEmpty(attr.fileName))
            {
                obj.name = type.Name;
            }
            else
            {
                obj.name = attr.fileName;
            }

            AssetDatabase.AddObjectToAsset(obj, mainAsset);
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(mainAsset));
            AssetDatabase.SaveAssets();

            EditorUtility.SetDirty(mainAsset);

            return obj;
        }

        public static ScriptableObject CreateToFolder(Type type, string folderPath)
        {
            var obj = ScriptableObject.CreateInstance(type);

            var attr = type.GetCustomAttribute<CreateAssetMenuAttribute>();
            if (attr == null || string.IsNullOrEmpty(attr.fileName))
            {
                obj.name = type.Name;
            }
            else
            {
                obj.name = attr.fileName;
            }

            AssetDatabase.CreateAsset(obj, AssetDatabase.GenerateUniqueAssetPath(folderPath + "/" + obj.name + ".asset"));
            AssetDatabase.SaveAssets();

            return obj;
        }

        public static void Duplicate(ScriptableObject asset)
        {
            if (asset == null) return;

            string path = AssetDatabase.GetAssetPath(asset);

            if (AssetDatabase.IsSubAsset(asset))
            {
                // Duplicate sub-asset
                ScriptableObject clone = Object.Instantiate(asset);
                clone.name = asset.name;

                Undo.RegisterCreatedObjectUndo(clone, "Duplicate Sub-Asset");
                AssetDatabase.AddObjectToAsset(clone, AssetDatabase.LoadMainAssetAtPath(path));
                AssetDatabase.ImportAsset(path);
                EditorUtility.SetDirty(clone);
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
            }
        }

        public static void Delete(ScriptableObject asset)
        {
            string mainPath = AssetDatabase.GetAssetPath(asset);

            if (!EditorUtility.DisplayDialog(
                "Delete Asset",
                $"Are you sure you want to delete '{asset.name}' from asset '{mainPath}'?",
                "Delete", "Cancel"))
                return;

            if (AssetDatabase.IsSubAsset(asset))
            {
                var parent = AssetDatabase.LoadMainAssetAtPath(mainPath);

                Undo.RegisterCompleteObjectUndo(parent, "Delete Sub-Asset");
                Object.DestroyImmediate(asset, true);
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(mainPath);
            }
            else
            {
                AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(asset));
                AssetDatabase.Refresh();
            }
        }

        public static void Rename(ScriptableObject asset, Action onComplete)
        {
            var target = asset;

            if (target == null) return;

            TextPromptWindow.Show("Rename", target.name, newName =>
            {
                Undo.RecordObject(target, "Rename");
                if (AssetDatabase.IsSubAsset(target))
                {
                    target.name = newName;
                    EditorUtility.SetDirty(target);
                }
                else
                {
                    AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(asset), newName);
                }

                AssetDatabase.SaveAssets();
                onComplete?.Invoke();
            });
        }
    }
}