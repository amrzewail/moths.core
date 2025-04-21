using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using Moths.Attributes;
using Moths.Collections;

using Moths.Editor.Collections;

namespace Moths.Editor.Attributes
{
    [CustomPropertyDrawer(typeof(MinMaxAttribute))]
    public class MinMaxAttributeDrawer : RangeFloatEditor
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

            //Debug.Log(property.type == typeof(RangeFloat).Name);

            //Debug.Log(this.fieldInfo.FieldType == typeof(RangeFloat));

            if (this.fieldInfo.FieldType == typeof(RangeFloat))
            {
                var attribute = this.attribute as MinMaxAttribute;

                var minProperty = property.FindPropertyRelative("min");
                var maxProperty = property.FindPropertyRelative("max");

                float min = Mathf.Max(minProperty.floatValue, attribute.Min);
                float max = Mathf.Min(maxProperty.floatValue, attribute.Max);

                minProperty.floatValue = Mathf.Min(min, max);
                maxProperty.floatValue = max;
            }

            base.OnGUI(position, property, label);

            //EditorGUI.PropertyField(position, property);
        }
    }
}