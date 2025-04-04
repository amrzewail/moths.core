using UnityEditor;
using UnityEngine;
using Moths.Fields;

namespace Moths.Editor.Fields
{
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

            var buttonRect = new Rect(position.x + position.width - 17.5f, position.y, 30, EditorGUIUtility.singleLineHeight);//EditorGUILayout.GetControlRect(false, 20f, GUILayout.MaxWidth(20f));

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
        }
    }
}