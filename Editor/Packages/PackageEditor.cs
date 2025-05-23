using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

using Moths.Internal.Packages;
using System.IO;

namespace Moths.Editor.Internal.Packages
{
    using Editor = UnityEditor.Editor;

    internal class Root : VisualElement
    {
        public Root(string label) : this()
        {
            var lbl = new Label(label);
            lbl.AddToClassList(BaseField<Label>.labelUssClassName);
            this.Add(lbl);
        }

        public Root()
        {
            this.style.display = DisplayStyle.Flex;
            this.style.flexDirection = FlexDirection.Row;
            this.style.justifyContent = Justify.SpaceBetween;
            this.style.paddingLeft = 3;
            this.AddToClassList(BaseField<VisualElement>.alignedFieldUssClassName);
        }
    }

    [CustomPropertyDrawer(typeof(Version))]
    internal class VersionDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new Root(property.displayName);

            var value = new VisualElement();
            value.style.flexDirection = FlexDirection.Row;
            root.Add(value);

            var v0 = new IntegerField();
            v0.style.width = 32;
            v0.BindProperty(property.FindPropertyRelative("v0"));

            var v1 = new IntegerField();
            v1.style.width = 32;
            v1.BindProperty(property.FindPropertyRelative("v1"));

            var v2 = new IntegerField();
            v2.style.width = 32;
            v2.BindProperty(property.FindPropertyRelative("v2"));

            value.Add(v0);
            value.Add(v1);
            value.Add(v2);

            return root;
        }
    }

    [CustomPropertyDrawer(typeof(Dependency))]
    internal class DependencyDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new Root();

            var name = new TextField();
            name.style.minWidth = 128;
            name.BindProperty(property.FindPropertyRelative("name"));

            root.Add(name);


            var version = new PropertyField(property.FindPropertyRelative("version"), string.Empty);
            root.Add(version);

            return root;
        }
    }

    [CustomPropertyDrawer(typeof(Change))]
    internal class ChangeDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var root = new VisualElement();
            root.style.paddingLeft = 3;

            var version = new PropertyField(property.FindPropertyRelative("version"), string.Empty);
            root.Add(version);

            var description = new PropertyField(property.FindPropertyRelative("description"), string.Empty);
            root.Add(description);

            return root;
        }
    }

    [CustomEditor(typeof(Package))]
    public class PackageEditor : Editor
    {

        public override VisualElement CreateInspectorGUI()
        {
            var root = new VisualElement();
            root.style.alignItems = Align.Center;

            root.Add(new PropertyField(serializedObject.FindProperty("_package")));

            var generateBtn = new Button();
            generateBtn.text = "Generate";

            generateBtn.style.marginTop = 24;
            generateBtn.style.width = 256;

            generateBtn.clicked += () =>
            {
                GenerateJson((Package)serializedObject.targetObject);
            };

            root.Add(generateBtn);

            return root;

        }


        private static void GenerateJson(Package package)
        {
            var assetPath = AssetDatabase.GetAssetPath(package);

            var directory = Path.GetDirectoryName(assetPath).Replace("\\", "/");

            var jsonPath = Path.Combine(Application.dataPath.Substring(0, Application.dataPath.Length - "Assets".Length), directory + "/package.json");

            System.IO.File.WriteAllText(jsonPath, package.ToString());

            AssetDatabase.Refresh();
        }

    }
}