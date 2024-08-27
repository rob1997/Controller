using Core.Common;
using UnityEditor;
using UnityEngine;

namespace Core.Editor
{
    [CustomPropertyDrawer(typeof(ExpandableAttribute))]
    public class ExpandablePropertyDrawer : PropertyDrawer
    {
        private UnityEditor.Editor _editor;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            float width = position.width;
            
            position.width = 0.05f * width;

            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
            
            position.x += position.width;
            
            position.width = width - position.width;
            
            EditorGUI.PropertyField(position, property, label, true);

            if (property.objectReferenceValue == null)
            {
                return;
            }
            
            if (property.isExpanded)
            {
                if (!_editor)
                {
                    UnityEditor.Editor.CreateCachedEditor(property.objectReferenceValue, null, ref _editor);
                }
                
                EditorGUILayout.BeginVertical(GUI.skin.box);
                
                _editor.OnInspectorGUI();
                
                EditorGUILayout.EndVertical();
            }
        }
    }
}