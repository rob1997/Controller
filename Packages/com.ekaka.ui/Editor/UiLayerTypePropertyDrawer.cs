using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ui.Main;
using Ui.Common;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Ui.Editor
{
    //list all layers as a dropdown
    [CustomPropertyDrawer(typeof(UiLayerTypeAttribute))]
    public class UiLayerTypePropertyDrawer : PropertyDrawer
    {
        private UiManager _uiManager;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_uiManager == null)
            {
                _uiManager = Object.FindObjectOfType<UiManager>();

                if (_uiManager == null)
                {
                    EditorGUI.HelpBox(position, $"{nameof(UiManager)} Instance not found", MessageType.Warning);

                    return;
                }
            }

            if (_uiManager.UiReferences == null)
            {
                EditorGUI.HelpBox(position, $"{nameof(UiReferences)} not referenced in {nameof(UiManager)} instance",
                    MessageType.Warning);

                return;
            }

            string[] array = _uiManager.UiReferences.UiLayerTypes;

            if (array.Length <= 0)
            {
                EditorGUI.HelpBox(position,
                    $"{nameof(UiReferences.UiLayerTypes)} list in {nameof(UiReferences)} Null or empty",
                    MessageType.Warning);

                return;
            }

            int index = Array.FindIndex(array, t => t == property.stringValue);

            if (index < 0) index = 0;

            index = EditorGUI.Popup(position, property.displayName, index, array);

            property.stringValue = array[index];
        }
    }
}