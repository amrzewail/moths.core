using UnityEditor;
using UnityEngine;

using Moths.Collections;
using System;
using System.Collections.Generic;

namespace Moths.Editor.Collections
{
    [CustomPropertyDrawer(typeof(Unique))]
    public class UniquePropertyEditor : PropertyDrawer
    {
        private static Dictionary<Unique, HashSet<int>> _uniques = new Dictionary<Unique, HashSet<int>>();

        private static System.Random _random = new System.Random();

        //public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        //{
        //    return 0;
        //}

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            EditorGUI.BeginProperty(position, label, property);

            var guid = property.FindPropertyRelative("_identifier");

            Unique target = (Unique)property.boxedValue;

            int hashCode = HashCode.Combine(property.propertyPath, property.serializedObject.targetObject.GetInstanceID());

            if (!_uniques.ContainsKey(target)) _uniques[target] = new HashSet<int>();

            _uniques[target].Add(hashCode);

            if (_uniques[target].Count >= 2)
            {
                _uniques[target].Remove(hashCode);
                guid.longValue = 0;
            }

            if (guid.longValue == 0)
            {
                guid.longValue = DateTime.UtcNow.Ticks + (_random.Next(1000));
            }


            EditorGUI.LabelField(position, guid.longValue.ToString());

            EditorGUI.EndProperty();
        }
    }
}