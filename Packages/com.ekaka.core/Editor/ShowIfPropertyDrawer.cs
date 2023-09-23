using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.Utils;
using UnityEditor;
using UnityEngine;

namespace Editor.Core
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute))]
    public class ShowIfPropertyDrawer : PropertyDrawer
    {
        private bool _showField;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            ShowIfAttribute showIfAttribute = (ShowIfAttribute) attribute;

            if (showIfAttribute == null)
            {
                return;
            }

            string conditionPath = showIfAttribute.ConditionPath;

            bool showIfCondition = showIfAttribute.ShowIfCondition;
            
            SerializedProperty conditionProperty = property.FindSiblingProperty(conditionPath);

            if (conditionProperty == null)
            {
                conditionProperty = property.FindSiblingProperty(conditionPath.GetPropertyName());

                if (conditionProperty == null)
                {
                    ShowError(position, $"Can't find property {conditionPath} in {property.serializedObject.targetObject}");
                
                    return;
                }
            }

            SerializedPropertyType conditionPropertyType = conditionProperty.propertyType;

            switch (conditionPropertyType)
            {
                case SerializedPropertyType.Boolean:
                    _showField = conditionProperty.boolValue == showIfCondition;
                    break;
                
                default:
                    ShowError(position, $"Property {conditionPath} is {conditionPropertyType}");
                    //return out after showing error
                    return;
            }
            
            if (_showField)
            {
                EditorGUI.PropertyField(position, property, label, true);

                property.serializedObject.ApplyModifiedProperties();
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (_showField)
            {
                return EditorGUI.GetPropertyHeight(property);
            }
            
            else
            {
                //hide property
                return 0;
            }
        }

        private void ShowError(Rect position, string errorText)
        {
            //enable show before showing error or it will be hidden
            _showField = true;
            
            EditorGUI.HelpBox(position, errorText, MessageType.Error);
        }
    }
}
