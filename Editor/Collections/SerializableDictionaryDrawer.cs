using UnityEditor;
using UnityEngine;
using static Codice.CM.Common.CmCallContext;

namespace Moths.Collections
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>.KeyValuePair))]
    public class SerializableDictionaryKeyValuePairDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty keyProp = property.FindPropertyRelative("key");
            SerializedProperty valueProp = property.FindPropertyRelative("value");

            float elementHeight = Mathf.Max(
                EditorGUI.GetPropertyHeight(keyProp),
                EditorGUI.GetPropertyHeight(valueProp));

            // Split rect in half for Key and Value
            float halfWidth = position.width / 2f;
            Rect keyRect = new Rect(position.x, position.y, halfWidth - 5, EditorGUI.GetPropertyHeight(keyProp));
            Rect valueRect = new Rect(position.x + halfWidth + 5, position.y, halfWidth - 5, EditorGUI.GetPropertyHeight(valueProp));

            // Draw the fields without labels
            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none, true);
            EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none, true);
        }
    }



    [CustomPropertyDrawer(typeof(SerializableDictionary<,>))]
    public class SerializableDictionaryDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position.width -= 2;

            SerializedProperty listProperty = property.FindPropertyRelative("_pairs");

            for (int i = 0; i < listProperty.arraySize; i++)
            {
                var currElementKey = listProperty.GetArrayElementAtIndex(i).FindPropertyRelative("key");

                for (int j = listProperty.arraySize - 1; j > i; j--)
                {
                    var nextElementKey = listProperty.GetArrayElementAtIndex(j).FindPropertyRelative("key");

                    if (currElementKey.boxedValue == null && nextElementKey.boxedValue == null)
                    {
                        listProperty.DeleteArrayElementAtIndex(j);
                    } 
                    else if (currElementKey.boxedValue.Equals(nextElementKey.boxedValue))
                    {
                        if (i == listProperty.arraySize - 2 && j == listProperty.arraySize - 1)
                        {
                            nextElementKey.boxedValue = default;
                        }
                        else
                        {
                            listProperty.DeleteArrayElementAtIndex(j);
                        }
                    }
                }
            }

            EditorGUI.PropertyField(new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight), listProperty, label, true);

            EditorGUI.EndProperty();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty listProperty = property.FindPropertyRelative("_pairs");
            return EditorGUI.GetPropertyHeight(listProperty);
        }
    }
}