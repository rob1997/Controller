using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.Core
{
    [CustomPropertyDrawer(typeof(ObjectTypeAttribute))]
    public class ObjectTypePropertyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedPropertyType propertyType = property.propertyType;

            if (propertyType != SerializedPropertyType.ObjectReference)
            {
                ShowError(position, $"{nameof(ObjectTypeAttribute)} can only be applied on {SerializedPropertyType.ObjectReference} type");
                
                return;
            }

            EditorGUI.PropertyField(position, property, label);
            
            ObjectTypeAttribute objectTypeAttribute = (ObjectTypeAttribute) attribute;

            Type typeToValidate = objectTypeAttribute.Type;
            
            Object objReference = property.objectReferenceValue;

            if (objReference != null)
            {
                //check validity
                bool isValid = ValidateType(objReference.GetType(), typeToValidate);

                //if it's a GameObject check for components on the obj with typeToValidate
                if (!isValid && objReference is GameObject obj)
                {
                    var component = obj.GetComponent(typeToValidate);
                    //if component exists set valid and assign property to component
                    if (component != null)
                    {
                        //since component already exists set valid to true
                        isValid = true;

                        property.objectReferenceValue = component;
                    }
                }

                if (!isValid)
                {
                    property.objectReferenceValue = null;
                    
                    Debug.LogWarning($"Type Mismatch, assigned type {objReference.GetType()} isn't a {typeToValidate} type");
                }
            }
            
            property.serializedObject.ApplyModifiedProperties();
        }

        //validate if objectType is typeToValidate
        private bool ValidateType(Type objectType, Type typeToValidate)
        {
            //this also checks subclasses, interfaces...
            return typeToValidate.IsAssignableFrom(objectType);
        }
        
        private void ShowError(Rect position, string error)
        {
            EditorGUI.HelpBox(position, error, MessageType.Error);
        }
    }
}
