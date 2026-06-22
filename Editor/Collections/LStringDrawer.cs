using Moths.Attributes;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Moths.Collections
{
    [CustomPropertyDrawer(typeof(LString), true)]
    public class LStringDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var useLocalization = property.FindPropertyRelative("_useLocalization");
            var fullRect = position;

            // Draw prefix and get remaining rect
            position = EditorGUI.PrefixLabel(position, label);

            float toggleWidth = 18f;
            float spacing = 4f;

            Rect toggleRect = new Rect(position.x + position.width - toggleWidth, position.y, toggleWidth, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = useLocalization.hasMultipleDifferentValues;
            bool newUseLocalization = EditorGUI.Toggle(toggleRect, GUIContent.none, useLocalization.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                useLocalization.boolValue = newUseLocalization;
            }
            EditorGUI.showMixedValue = false;

            if (useLocalization.hasMultipleDifferentValues)
            {
                Rect valueRect = new Rect(
                    position.x,
                    position.y,
                    toggleRect.x - position.x - spacing,
                    position.height
                );
                EditorGUI.showMixedValue = true;
                EditorGUI.TextField(valueRect, "");
                EditorGUI.showMixedValue = false;
            }
            else if (useLocalization.boolValue)
            {
                var localization = property.FindPropertyRelative("_localization");

                fullRect.width -= toggleWidth + spacing;

                EditorGUI.PropertyField(fullRect, localization, GUIContent.none); 
            }
            else
            {
                float buttonWidth = 22f;
                Rect buttonRect = new Rect(toggleRect.x - buttonWidth - spacing, position.y, buttonWidth, EditorGUIUtility.singleLineHeight);
                Rect valueRect = new Rect(
                    position.x,
                    position.y,
                    buttonRect.x - position.x - spacing,
                    position.height
                );

                var text = property.FindPropertyRelative("_text");
                valueRect.height = GetTextAreaHeight(text.stringValue);

                EditorGUI.BeginChangeCheck();
                EditorGUI.showMixedValue = text.hasMultipleDifferentValues;
                string newText = EditorGUI.TextArea(valueRect, text.stringValue);
                if (EditorGUI.EndChangeCheck())
                {
                    text.stringValue = newText;
                }
                EditorGUI.showMixedValue = false;

                if (GUI.Button(buttonRect, new GUIContent("...", "Edit text in a resizable window"), EditorStyles.miniButton))
                {
                    LStringTextEditorWindow.ShowWindow(text);
                }
            }

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var useLocalization = property.FindPropertyRelative("_useLocalization");
            
            if (useLocalization.hasMultipleDifferentValues)
            {
                return EditorGUIUtility.singleLineHeight;
            }

            if (useLocalization.boolValue)
            {
                return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_localization"), label, true) - 2;
            }

            var text = property.FindPropertyRelative("_text");

            return GetTextAreaHeight(text.stringValue);
        }

        private float GetTextAreaHeight(string text)
        {
            int lines = Mathf.Max(1, text.Split('\n').Length);
            lines = Mathf.Clamp(lines, 1, 8);

            return lines * EditorGUIUtility.singleLineHeight - 3.5f * (lines - 1);
        }
    }

    public class LStringTextEditorWindow : EditorWindow
    {
        private SerializedObject serializedObject;
        private string propertyPath;
        private string displayName;

        public static void ShowWindow(SerializedProperty property)
        {
            var window = GetWindow<LStringTextEditorWindow>(true, "Edit Text", true);
            window.serializedObject = property.serializedObject;
            window.propertyPath = property.propertyPath;
            window.displayName = property.displayName;
            window.minSize = new Vector2(400, 300);
            window.Show();
            window.Repaint();
        }

        private void OnGUI()
        {
            if (serializedObject == null || serializedObject.targetObject == null)
            {
                EditorGUILayout.HelpBox("No active property selected.", MessageType.Info);
                return;
            }

            serializedObject.Update();
            SerializedProperty prop = serializedObject.FindProperty(propertyPath);
            if (prop == null)
            {
                EditorGUILayout.HelpBox("Property not found.", MessageType.Error);
                return;
            }

            // Header Section
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(prop.displayName, EditorStyles.boldLabel);
            
            string targetName = serializedObject.targetObjects.Length > 1 
                ? $"{serializedObject.targetObjects.Length} Objects" 
                : serializedObject.targetObject.name;
            EditorGUILayout.LabelField($"{targetName} > {displayName}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Text Area
            EditorGUI.BeginChangeCheck();
            GUIStyle textAreaStyle = new GUIStyle(EditorStyles.textArea);
            textAreaStyle.wordWrap = true;
            textAreaStyle.fontSize = 12;
            textAreaStyle.padding = new RectOffset(8, 8, 8, 8);
            
            string currentVal = prop.stringValue ?? "";
            EditorGUI.showMixedValue = prop.hasMultipleDifferentValues;
            string newText = EditorGUILayout.TextArea(currentVal, textAreaStyle, GUILayout.ExpandHeight(true));
            EditorGUI.showMixedValue = false;
            
            if (EditorGUI.EndChangeCheck())
            {
                prop.stringValue = newText;
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space(5);

            // Status Bar at the bottom
            int charCount = currentVal.Length;
            int wordCount = string.IsNullOrEmpty(currentVal) ? 0 : currentVal.Split(new char[] { ' ', '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries).Length;
            int lineCount = string.IsNullOrEmpty(currentVal) ? 0 : currentVal.Split('\n').Length;

            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            if (prop.hasMultipleDifferentValues)
            {
                EditorGUILayout.LabelField("Mixed values", EditorStyles.miniLabel);
            }
            else
            {
                EditorGUILayout.LabelField($"Length: {charCount} | Words: {wordCount} | Lines: {lineCount}", EditorStyles.miniLabel);
            }
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("Clear", EditorStyles.miniButton, GUILayout.Width(50)))
            {
                if (EditorUtility.DisplayDialog("Clear Text", "Are you sure you want to clear the text?", "Yes", "No"))
                {
                    prop.stringValue = "";
                    serializedObject.ApplyModifiedProperties();
                }
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}