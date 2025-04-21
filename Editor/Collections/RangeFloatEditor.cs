using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

using Moths.Collections;

namespace Moths.Editor.Collections
{

    [CustomPropertyDrawer(typeof(RangeFloat))]
    public class RangeFloatEditor : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            float width = position.width * 0.5f - 17;

            var fromRect = new Rect(position.x, position.y, 50, position.height);
            var toRect = new Rect(position.x + 55, position.y, 50, position.height);


            float labelWidthTmp = EditorGUIUtility.labelWidth;

            EditorGUIUtility.labelWidth = 16f * 2f;

            position.width = width;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("min"), new GUIContent("Min"));

            position.x += width + 10;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("max"), new GUIContent("Max"));


            EditorGUIUtility.labelWidth = labelWidthTmp;

            EditorGUI.EndProperty();
        }
    }
}