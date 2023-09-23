using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using UnityEditor;
using UnityEngine;

namespace Editor.Core
{
    [CustomPropertyDrawer(typeof(SceneListAttribute))]
    public class SceneListPropertyDrawer : PropertyDrawer
    {
        private string[] _sceneNames;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (_sceneNames == null || _sceneNames.Length == 0)
            {
                _sceneNames = Utils.GetAllSceneNamesInBuildSettings();
            }

            property.intValue = EditorGUI.Popup(position, property.displayName, Mathf.Clamp(property.intValue, 0, _sceneNames.MaxIndex()), _sceneNames);
        }
    }
}
