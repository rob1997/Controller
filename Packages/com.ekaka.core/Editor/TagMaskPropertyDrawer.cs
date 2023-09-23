using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using Editor.Core;
using UnityEditor;
using UnityEngine;

namespace Editor.Core
{
    [CustomPropertyDrawer(typeof(TagMask))]
    public class TagMaskPropertyDrawer : PropertyDrawer
    {
        private Rect _position;

        private bool _foldout;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _position = position;

            _position.height = EditorGUIUtility.singleLineHeight;

            _foldout = EditorGUI.Foldout(_position, _foldout, label);

            if (_foldout)
            {
                MovePositionDown();

                bool changesMade = DrawTagMask(position, property);

                if (changesMade) property.serializedObject.ApplyModifiedProperties();
            }

            _position.y -= position.y;
        }

        private bool DrawTagMask(Rect position, SerializedProperty property)
        {
            string[] allTags = UnityEditorInternal.InternalEditorUtility.tags;

            SerializedProperty selectedTagsProperty =
                property.FindPropertyRelative(nameof(TagMask.SelectedTags).GetPropertyName());

            bool changesMade = false;

            CheckTags(selectedTagsProperty, allTags, ref changesMade);

            for (int i = 0; i < allTags.Length; i++)
            {
                string tag = allTags[i];

                _position.width = BaseEditor.SmallButtonWidth;

                bool isTagSelected = ContainsTag(selectedTagsProperty, tag);

                if (EditorGUI.Toggle(_position, isTagSelected))
                {
                    //add tag
                    if (!isTagSelected)
                    {
                        AddTag(selectedTagsProperty, tag);

                        changesMade = true;
                    }
                }

                else
                {
                    //remove tag
                    if (isTagSelected)
                    {
                        RemoveTag(selectedTagsProperty, tag);

                        changesMade = true;
                    }
                }

                //move to right
                _position.x += _position.width;
                //set new width
                _position.width = position.width - _position.width;

                EditorGUI.LabelField(_position, tag);

                //revert values
                _position.x = position.x;

                _position.width = position.width;

                MovePositionDown();
            }

            return changesMade;
        }

        //moves down position
        private void MovePositionDown()
        {
            //moveDown
            _position.y += _position.height;
        }

        //checks if there's any string values in TagMask.SelectedTags that's not an actual tag (was removed after being selected)
        private void CheckTags(SerializedProperty selectedTagsProperty, string[] allTags, ref bool changesMade)
        {
            for (int i = 0; i < selectedTagsProperty.arraySize; i++)
            {
                SerializedProperty tagProperty = selectedTagsProperty.GetArrayElementAtIndex(i);

                //remove uncleaned tags (selected but removed tags)
                if (!allTags.Contains(tagProperty.stringValue))
                {
                    selectedTagsProperty.DeleteArrayElementAtIndex(i);

                    changesMade = true;
                }
            }
        }

        private bool ContainsTag(SerializedProperty selectedTagsProperty, string tag)
        {
            for (int i = 0; i < selectedTagsProperty.arraySize; i++)
            {
                SerializedProperty tagProperty = selectedTagsProperty.GetArrayElementAtIndex(i);

                if (tagProperty.stringValue == tag)
                {
                    return true;
                }
            }

            return false;
        }

        private void RemoveTag(SerializedProperty selectedTagsProperty, string tag)
        {
            for (int i = 0; i < selectedTagsProperty.arraySize; i++)
            {
                SerializedProperty tagProperty = selectedTagsProperty.GetArrayElementAtIndex(i);

                if (tagProperty.stringValue == tag)
                {
                    selectedTagsProperty.DeleteArrayElementAtIndex(i);

                    return;
                }
            }
        }

        private void AddTag(SerializedProperty selectedTagsProperty, string tag)
        {
            int newIndex = selectedTagsProperty.arraySize;

            selectedTagsProperty.InsertArrayElementAtIndex(newIndex);

            SerializedProperty tagProperty = selectedTagsProperty.GetArrayElementAtIndex(newIndex);

            tagProperty.stringValue = tag;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _foldout ? _position.y : _position.height;
        }
    }
}