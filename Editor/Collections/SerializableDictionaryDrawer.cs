using System;
using UnityEditor;
using UnityEngine;

namespace Moths.Collections
{
    [CustomPropertyDrawer(typeof(SerializableDictionary<,>.KeyValuePair))]
    public class SerializableDictionaryKeyValuePairDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty keyProp = property.FindPropertyRelative("key");
            SerializedProperty valueProp = property.FindPropertyRelative("value");

            Type valueType = GetValueType();
            bool isManaged = IsManagedObject(valueProp, valueType);

            float keyHeight = EditorGUI.GetPropertyHeight(keyProp, true);
            float valueHeight = isManaged 
                ? EditorGUIUtility.singleLineHeight 
                : EditorGUI.GetPropertyHeight(valueProp, true);

            // Split rect in half for Key and Value
            float halfWidth = position.width / 2f;
            Rect keyRect = new Rect(position.x, position.y, halfWidth - 5, keyHeight);
            Rect valueRect = new Rect(position.x + halfWidth + 5, position.y, halfWidth - 5, valueHeight);

            // Draw the fields without labels
            EditorGUI.PropertyField(keyRect, keyProp, GUIContent.none, true);

            if (isManaged)
            {
                string buttonText = $"Edit {valueProp.type}";
                if (GUI.Button(valueRect, new GUIContent(buttonText, $"Edit this {valueProp.type} in a separate window"), EditorStyles.miniButton))
                {
                    SerializableDictionaryObjectEditorWindow.ShowWindow(valueProp);
                }
            }
            else
            {
                EditorGUI.PropertyField(valueRect, valueProp, GUIContent.none, true);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            SerializedProperty keyProp = property.FindPropertyRelative("key");
            SerializedProperty valueProp = property.FindPropertyRelative("value");

            Type valueType = GetValueType();
            float valueHeight = IsManagedObject(valueProp, valueType) 
                ? EditorGUIUtility.singleLineHeight 
                : EditorGUI.GetPropertyHeight(valueProp, true);

            return Mathf.Max(EditorGUI.GetPropertyHeight(keyProp, true), valueHeight);
        }

        private Type GetValueType()
        {
            if (fieldInfo == null) return null;
            Type fieldType = fieldInfo.FieldType;
            if (fieldType.IsArray)
            {
                fieldType = fieldType.GetElementType();
            }
            else if (fieldType.IsGenericType && fieldType.GetGenericTypeDefinition() == typeof(System.Collections.Generic.List<>))
            {
                fieldType = fieldType.GetGenericArguments()[0];
            }

            Type currentType = fieldType.DeclaringType;
            while (currentType != null)
            {
                if (currentType.IsGenericType && currentType.GetGenericTypeDefinition() == typeof(SerializableDictionary<,>))
                {
                    return currentType.GetGenericArguments()[1];
                }
                currentType = currentType.BaseType;
            }
            return null;
        }

        private bool IsManagedObject(SerializedProperty property, Type type)
        {
            if (property == null) return false;
            if (property.propertyType == SerializedPropertyType.Generic ||
                property.propertyType == SerializedPropertyType.ManagedReference)
            {
                string typeName = property.type;
                if (typeName == "Vector2" || typeName == "Vector3" || typeName == "Vector4" ||
                    typeName == "Vector2Int" || typeName == "Vector3Int" ||
                    typeName == "Color" || typeName == "Rect" || typeName == "RectInt" ||
                    typeName == "Bounds" || typeName == "BoundsInt" || typeName == "Quaternion")
                {
                    return false;
                }
                return property.hasChildren;
            }
            return false;
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

    public class SerializableDictionaryObjectEditorWindow : EditorWindow
    {
        private SerializedObject serializedObject;
        private string propertyPath;
        private string displayName;
        private Vector2 scrollPosition;

        public static void ShowWindow(SerializedProperty property)
        {
            var window = GetWindow<SerializableDictionaryObjectEditorWindow>(true, "Edit Object", true);
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
            string keyPath = "";
            if (propertyPath.EndsWith(".value"))
            {
                keyPath = propertyPath.Substring(0, propertyPath.Length - 6) + ".key";
            }
            string keyString = "";
            if (!string.IsNullOrEmpty(keyPath))
            {
                SerializedProperty keyProp = serializedObject.FindProperty(keyPath);
                if (keyProp != null)
                {
                    if (keyProp.propertyType == SerializedPropertyType.ObjectReference)
                    {
                        keyString = keyProp.objectReferenceValue != null ? keyProp.objectReferenceValue.name : "None";
                    }
                    else
                    {
                        keyString = keyProp.boxedValue?.ToString() ?? "null";
                    }
                }
            }

            EditorGUILayout.LabelField("Editing Dictionary Value", EditorStyles.boldLabel);
            if (!string.IsNullOrEmpty(keyString))
            {
                EditorGUILayout.LabelField($"Key: {keyString}", EditorStyles.label);
            }

            string targetName = serializedObject.targetObjects.Length > 1
                ? $"{serializedObject.targetObjects.Length} Objects"
                : serializedObject.targetObject.name;
            EditorGUILayout.LabelField($"{targetName} > {displayName}", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(5);

            // Scroll View for object fields
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            EditorGUI.BeginChangeCheck();

            bool hasChildren = false;
            SerializedProperty endProp = prop.GetEndProperty();
            SerializedProperty childProp = prop.Copy();
            if (childProp.NextVisible(true))
            {
                do
                {
                    if (SerializedProperty.EqualContents(childProp, endProp))
                        break;
                    hasChildren = true;
                    EditorGUILayout.PropertyField(childProp, true);
                } while (childProp.NextVisible(false));
            }

            if (!hasChildren)
            {
                EditorGUILayout.PropertyField(prop, true);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.EndScrollView();
        }
    }
}