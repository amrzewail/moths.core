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

            Rect toggleRect = new Rect(position.x + position.width - toggleWidth, position.y, toggleWidth, position.height);
            Rect valueRect = new Rect(
                position.x,
                position.y,
                position.width - toggleWidth - spacing,
                position.height
            );

            useLocalization.boolValue = EditorGUI.Toggle(toggleRect, GUIContent.none, useLocalization.boolValue);

            if (useLocalization.boolValue)
            {
                var localization = property.FindPropertyRelative("_localization");

                fullRect.width -= toggleWidth + spacing;

                EditorGUI.PropertyField(fullRect, localization, GUIContent.none); 
            }
            else
            {
                var text = property.FindPropertyRelative("_text");
                valueRect.height = GetTextAreaHeight(text.stringValue);
                text.stringValue = EditorGUI.TextArea(valueRect, text.stringValue);
            }

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var useLocalization = property.FindPropertyRelative("_useLocalization");
            
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
}