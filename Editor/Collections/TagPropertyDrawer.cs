using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Moths.Collections;

namespace Moths.Editor.Collections
{
    [CustomPropertyDrawer(typeof(Tag))]
    public class TagPropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty tagProperty = property.FindPropertyRelative("tag");

            EditorGUI.BeginProperty(position, label, tagProperty);

            EditorGUI.PropertyField(position, tagProperty);

            EditorGUI.EndProperty();
        }
    }
}