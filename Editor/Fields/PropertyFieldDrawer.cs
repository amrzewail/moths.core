using UnityEditor;
using UnityEngine;
using System.Reflection;
using Moths.Fields;

namespace Moths.Editor.Fields
{
    [CustomEditor(typeof(GenericField), true)]
    public class GenericFieldDrawer : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty property = serializedObject.GetIterator();

            SerializedProperty stacktracesProperty = null;

            bool hideValue = false;
            bool hideStacktrace = false;

            foreach(var attr in base.target.GetType().GetCustomAttributes())
            {
                if (attr is GenericField.HideValueAttribute) hideValue = true;
                if (attr is GenericField.HideStacktraceAttribute) hideStacktrace = true;
            }

            if (property.NextVisible(true))
            {
                do
                {
                    if (property.propertyPath == "m_Script") continue;

                    if (hideValue && property.name == "value") continue;

                    if (!hideStacktrace && property.name == "_stacktraces")
                    {
                        stacktracesProperty = property.Copy();
                        continue;
                    }
                    EditorGUILayout.PropertyField(property, true);
                }
                while (property.NextVisible(false));
            }

            if (stacktracesProperty != null)
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.PropertyField(stacktracesProperty, true);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }


    [CustomPropertyDrawer(typeof(GenericReference), true)]
    public class GenericPropertyFieldDrawer : PropertyDrawer
    {

        private string[] _options = null;
        private GUIStyle _style = null;

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {

            var valueType = property.FindPropertyRelative("valueType");

            if (valueType.enumValueIndex == (int)Type.Value)
            {
                var p = property.FindPropertyRelative("value");
                return EditorGUI.GetPropertyHeight(p, true);
            }

            return base.GetPropertyHeight(property, label);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.

            var valueType = property.FindPropertyRelative("valueType");

            EditorGUI.BeginProperty(position, label, property);

            if (_options == null)
            {
                _options = new string[3];
                for (int i = 0; i < _options.Length; i++) _options[i] = ((Type)i).ToString();
            }

            if (_style == null)
            {
                _style = new GUIStyle(GUI.skin.FindStyle("IconButton"));
                var popupIcon = EditorGUIUtility.IconContent("_Popup");
                _style.normal.textColor = Color.clear;
                _style.normal.background = (Texture2D)popupIcon.image;

            }

            var buttonRect = new Rect(position.x + position.width - 17f, position.y, 30, EditorGUIUtility.singleLineHeight);//EditorGUILayout.GetControlRect(false, 20f, GUILayout.MaxWidth(20f));

            buttonRect.x -= 17f * Mathf.Max(0, Mathf.Sign((property.depth - 1)));

            valueType.enumValueIndex = EditorGUI.Popup(buttonRect, valueType.enumValueIndex, _options, _style);

            position.width -= 20;

            switch (valueType.enumValueIndex)
            {
                case (int)Type.Value:
                    EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), label, true);

                    break;
                case (int)Type.Field:
                    EditorGUI.PropertyField(position, property.FindPropertyRelative("field"), label);

                    break;
                case (int)Type.MonoBehaviour:
                    EditorGUI.PropertyField(position, property.FindPropertyRelative("monoBehaviour"), label, true);
                    break;
            }

            EditorGUI.EndProperty();


            GenericReference target = (GenericReference)property.boxedValue;
            target.OnValidate();

            property.serializedObject.ApplyModifiedProperties();
        }
    }
}