using System.Dynamic;
using Core.Common;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(SerializedDictionaryAttribute), true)]
    public class SerializedDictionaryPropertyDrawer : PropertyDrawer
    {
        private Rect _position;
        
        private bool _foldout;
        
        private readonly float _foldoutPadding = 15f;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _position = position;
            
            _position.height = EditorGUIUtility.singleLineHeight;

            _foldout = EditorGUI.Foldout(_position, _foldout, new GUIContent(property.displayName, property.tooltip));
            
            if (_foldout)
            {
                _position.x += _foldoutPadding;

                _position.width -= _foldoutPadding;
                
                NewLine();
                
                DrawSerializedDictionary(property);
                
                property.serializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawSerializedDictionary(SerializedProperty dictProperty)
        {
            SerializedProperty pairListProperty = dictProperty.FindPropertyRelative(BaseEditor.SerializedListName);

            EditorGUI.LabelField(MovedRect(0f, .4f), "Keys", EditorStyles.boldLabel);
            
            EditorGUI.LabelField(MovedRect(.45f, .4f), "Values", EditorStyles.boldLabel);
            
            if (GUI.Button(MovedRect(.9f, .1f), new GUIContent("+", "Add")))
            {
                pairListProperty.InsertArrayElementAtIndex(pairListProperty.arraySize);
            }
            
            NewLine(1.25f);

            int index = 0;
            
            foreach (SerializedProperty property in pairListProperty)
            {
                SerializedProperty keyProperty = property.FindPropertyRelative(BaseEditor.KeyName);

                EditorGUI.PropertyField(MovedRect(0f, .4f), keyProperty, GUIContent.none);
                
                SerializedProperty valueProperty = property.FindPropertyRelative(BaseEditor.ValueName);

                EditorGUI.PropertyField(MovedRect(.45f, .4f), valueProperty, GUIContent.none);
                
                if (GUI.Button(MovedRect(.9f, .1f), new GUIContent("-", "Remove")))
                {
                    pairListProperty.DeleteArrayElementAtIndex(index);
                }

                NewLine();

                index++;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _foldout ? _position.y : _position.height;
        }

        private Rect MovedRect(float normalizedX = 1f, float normalizedWidth = 1f)
        {
            Rect rect = _position;

            rect.x += normalizedX * _position.width;
            
            rect.width *= normalizedWidth;
            
            return rect;
        }
        
        private void NewLine(float normalizedLineHeight = 1.15f)
        {
            _position.y += _position.height * normalizedLineHeight;
        }
    }
}
