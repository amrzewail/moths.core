using UnityEditor;
using UnityEngine;

namespace Moths.Collections
{
    [CustomPropertyDrawer(typeof(OptionalProperty<>), true)]
    public class OptionalPropertyDrawer : PropertyDrawer
    {
        private const float ToggleWidth = 18f;
        private const float Spacing = 2f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            var isEnabled = property.FindPropertyRelative("_isEnabled");
            var value = property.FindPropertyRelative("_value");

            // Draw prefix and get remaining rect
            position = EditorGUI.PrefixLabel(position, label);

            float toggleWidth = 18f;
            float spacing = 4f;

            Rect toggleRect = new Rect(position.x, position.y, toggleWidth, position.height);
            Rect valueRect = new Rect(
                toggleRect.xMax + spacing,
                position.y,
                position.width - toggleWidth - spacing,
                position.height
            );

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = isEnabled.hasMultipleDifferentValues;
            bool newIsEnabled = EditorGUI.Toggle(toggleRect, GUIContent.none, isEnabled.boolValue);
            if (EditorGUI.EndChangeCheck())
            {
                isEnabled.boolValue = newIsEnabled;
            }
            EditorGUI.showMixedValue = false;

            using (new EditorGUI.DisabledScope(!isEnabled.boolValue))
            {
                EditorGUI.PropertyField(valueRect, value, GUIContent.none);
            }

            EditorGUI.EndProperty();
        }


        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var isOverriden = property.FindPropertyRelative("_isEnabled");
            var valueProp = property.FindPropertyRelative("_value");
            return EditorGUI.GetPropertyHeight(valueProp, label, true);
        }
    }
}