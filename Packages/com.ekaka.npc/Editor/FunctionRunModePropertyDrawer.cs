using System.Collections;
using System.Collections.Generic;
using Editor.Core;
using NPC.Main;
using UnityEditor;
using UnityEngine;

namespace NPC.Editor
{
    [CustomPropertyDrawer(typeof(FunctionRunMode))]
    public class FunctionRunModePropertyDrawer : PropertyDrawer
    {
        private const float Space = 10f;
        
        private Rect _position;
        
        private bool _foldout;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _position = position;

            _position.height = EditorGUIUtility.singleLineHeight;

            _foldout = EditorGUI.Foldout(_position, _foldout, label);

            if (_foldout)
            {
                MoveRectDown();
                
                SerializedProperty runModeTypeProperty = property.FindPropertyRelative(nameof(FunctionRunMode.RunModeType).GetPropertyName());

                EditorGUI.PropertyField(_position, runModeTypeProperty, new GUIContent(runModeTypeProperty.displayName, runModeTypeProperty.tooltip));
                
                if (runModeTypeProperty.GetEnumValue(out FunctionRunModeType runModeType) && runModeType == FunctionRunModeType.Custom)
                {
                    DrawCustomRunModeTypeFields(property);
                }
            }

            MoveRectDown();
            
            _position.y -= position.y;

            property.serializedObject.ApplyModifiedProperties();
        }

        private void DrawRelativeProperty(SerializedProperty property, string relativePath)
        {
            SerializedProperty relativeProperty = property.FindPropertyRelative(relativePath.GetPropertyName());
            
            MoveRectDown();

            EditorGUI.PropertyField(_position, relativeProperty,
                new GUIContent(relativeProperty.displayName, relativeProperty.tooltip));
        }
        
        private void DrawCustomRunModeTypeFields(SerializedProperty runModeProperty)
        {
            
            DrawRelativeProperty(runModeProperty, nameof(FunctionRunMode.Delay));
            
            DrawRelativeProperty(runModeProperty, nameof(FunctionRunMode.Interval));
            
            DrawRelativeProperty(runModeProperty, nameof(FunctionRunMode.Frequency));
        }

        private void MoveRectDown()
        {
            _position.y += _position.height;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _foldout ? _position.y + Space : _position.height;
        }
    }
}