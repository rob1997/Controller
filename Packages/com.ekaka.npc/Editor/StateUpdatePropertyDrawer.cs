using System.Collections;
using System.Collections.Generic;
using Core.Editor;
using NPC.Main;
using UnityEditor;
using UnityEngine;

namespace NPC.Editor
{
    [CustomPropertyDrawer(typeof(StateUpdate))]
    public class StateUpdatePropertyDrawer : PropertyDrawer
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
                
                SerializedProperty updateTypeProperty = property.FindPropertyRelative(nameof(StateUpdate.UpdateType).GetPropertyName());

                EditorGUI.PropertyField(_position, updateTypeProperty, new GUIContent(updateTypeProperty.displayName, updateTypeProperty.tooltip));
                
                if (updateTypeProperty.GetEnumValue(out StateUpdateType updateType) && updateType == StateUpdateType.Custom)
                {
                    DrawCustomUpdateTypeFields(property);
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
        
        private void DrawCustomUpdateTypeFields(SerializedProperty stateUpdateProperty)
        {
            
            DrawRelativeProperty(stateUpdateProperty, nameof(StateUpdate.Delay));
            
            DrawRelativeProperty(stateUpdateProperty, nameof(StateUpdate.Interval));
            
            DrawRelativeProperty(stateUpdateProperty, nameof(StateUpdate.Frequency));
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