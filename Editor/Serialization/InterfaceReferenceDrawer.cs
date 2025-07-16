using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Moths.Serialization
{
    [CustomPropertyDrawer(typeof(InterfaceReference), true)]
    public class InterfaceReferenceDrawer : PropertyDrawer
    {
        private Type _interfaceType;
        private List<Type> _implementedTypes;
        private string[] _implementedTypesNames;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property.FindPropertyRelative("_object"), true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_interfaceType == null) BuildTypes(property);

            EditorGUI.BeginChangeCheck();

            DrawProperty(position, property.FindPropertyRelative("_object"), label);

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }


        private void BuildTypes(SerializedProperty property)
        {
            if (property.boxedValue == null) return;

            _interfaceType = ((InterfaceReference)property.boxedValue).GetInterfaceType();

            var type = _interfaceType;
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => !p.IsInterface && !p.IsAbstract && p != type && type.IsAssignableFrom(p));

            _implementedTypes = types.ToList();
            _implementedTypesNames = _implementedTypes.Select(t => t.Name).Prepend("Null").ToArray();
        }

        private void DrawProperty(Rect position, SerializedProperty property, GUIContent label)
        {
            int selected = -1;

            if (property.managedReferenceValue == null)
            {
                selected = 0;
            }
            else
            {
                selected = _implementedTypes.IndexOf(property.managedReferenceValue.GetType()) + 1;
            }

            int newSelected = EditorGUI.Popup(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), label.text, selected, _implementedTypesNames);

            if (newSelected > 0 && newSelected != selected)
            {
                selected = newSelected;
                property.managedReferenceValue = Activator.CreateInstance(_implementedTypes[selected - 1]);
            }
            else if (newSelected == 0)
            {
                property.managedReferenceValue = null;
            }

            if (property.managedReferenceValue != null)
            {
                EditorGUI.PropertyField(position, property, new GUIContent(""), true);
            }
        }
    }
}