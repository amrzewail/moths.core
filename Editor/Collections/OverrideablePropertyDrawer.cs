using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Moths.Collections
{
    [CustomPropertyDrawer(typeof(OverrideableProperty<>), true)]
    public class OverrideablePropertyDrawer : PropertyDrawer
    {
        private const float ToggleWidth = 18f;
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var isOverriden = property.FindPropertyRelative("_isOverriden");
            var @override = property.FindPropertyRelative("_override");
            var value = property.FindPropertyRelative("_value");

            Rect valueRect = new Rect(position.x, position.y, position.width - ToggleWidth - Spacing, position.height);
            EditorGUI.PropertyField(valueRect, isOverriden.boolValue ? @override : value, new GUIContent(label + (isOverriden.boolValue ? " (Overriden)" : "")), true);

            Rect toggleRect = new(position.x + position.width - ToggleWidth - 11, position.y, ToggleWidth, position.height);
            isOverriden.boolValue = EditorGUI.Toggle(toggleRect, isOverriden.boolValue);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var isOverriden = property.FindPropertyRelative("_isOverriden");
            var valueProp = property.FindPropertyRelative("_value");
            var overrideProp = property.FindPropertyRelative("_override");

            return EditorGUI.GetPropertyHeight(
                isOverriden.boolValue ? overrideProp : valueProp,
                label,
                true
            );
        }
    }
}