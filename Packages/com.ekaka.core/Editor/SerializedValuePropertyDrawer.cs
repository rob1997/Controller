using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using UnityEditor;
using UnityEngine;

namespace Editor.Core
{
    [CustomPropertyDrawer(typeof(SerializedValue<>))]
    public class SerializedValuePropertyDrawer : PropertyDrawer
    {
        private Rect _position;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _position = position;

            _position.height = EditorGUIUtility.singleLineHeight;
            
            if (DrawValueProperty(property))
            {
                property.serializedObject.ApplyModifiedProperties();
            }

            _position.y -= position.y;
        }

        private bool DrawValueProperty(SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();

            SerializedProperty valueProperty = property.FindPropertyRelative(nameof(SerializedValue<Component>.Value).GetPropertyName());
            
            EditorGUI.PropertyField(_position, valueProperty, new GUIContent(property.displayName, property.tooltip));

            //move position down
            _position.y += _position.height;
            
            return EditorGUI.EndChangeCheck();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _position.y;
        }
    }
}
