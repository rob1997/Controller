using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using Editor.Core;
using Ui.Main;
using UnityEditor;
using UnityEngine;

namespace Ui.Editor
{
    [CustomPropertyDrawer(typeof(UiModalData))]
    public class UiModalDataPropertyDrawer : PropertyDrawer
    {
        //this is needed because serializedProperty iterator iterates through a lot of non visible and unnecessary properties
        private readonly string[] _toDrawProperties =
        {
            nameof(UiModalData.Title), nameof(UiModalData.Body), nameof(UiModalData.UiModalActions),
            nameof(UiModalData.ForcePick), nameof(UiModalData.BaseRect), nameof(UiModalData.UseDefaultPadding), nameof(UiModalData.Padding),
            nameof(UiModalData.UseDefaultUiModal), nameof(UiModalData.UiModalMenuType), nameof(UiModalData.Unloaded),
        };

        private Rect _position;

        private bool _foldout;

        private bool _useDefaultUiModal;

        private int _uiModalActionsLength;

        private const float DefaultPadding = 5f;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _position = position;

            _position.height = EditorGUIUtility.singleLineHeight;

            _foldout = EditorGUI.Foldout(_position, _foldout, label);

            _position.y += _position.height;

            if (!_foldout)
            {
                return;
            }

            //add indent on the left and padding on the right
            _position.x += DefaultPadding;
            _position.width -= DefaultPadding * 2f;

            foreach (SerializedProperty subProperty in property)
            {
                //make sure it's a property we want to draw
                if (!_toDrawProperties.Contains(subProperty.name.GetNameFromPropertyName()))
                {
                    continue;
                }

                switch (subProperty.name.GetNameFromPropertyName())
                {
                    case nameof(UiModalData.UseDefaultUiModal):
                        _useDefaultUiModal = subProperty.boolValue;
                        break;

                    case nameof(UiModalData.UiModalActions):
                        _uiModalActionsLength = subProperty.arraySize;
                        break;

                    case nameof(UiModalData.ForcePick):
                        if (_uiModalActionsLength <= 0)
                        {
                            continue;
                        }

                        break;

                    case nameof(UiModalData.UiModalMenuType):
                        if (_useDefaultUiModal)
                        {
                            continue;
                        }

                        break;
                }

                //draw property, update position and height
                _position.height = EditorGUI.GetPropertyHeight(subProperty, true);

                EditorGUI.PropertyField(_position, subProperty, new GUIContent(subProperty.displayName, subProperty.tooltip),
                    true);
                //update vertical position with padding
                _position.y += _position.height + DefaultPadding;
            }

            //deduct difference so _position.y can be equal to real height of property
            _position.y -= position.y;
            
            property.serializedObject.ApplyModifiedProperties();
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _foldout ? _position.y : EditorGUIUtility.singleLineHeight;
        }
    }
}