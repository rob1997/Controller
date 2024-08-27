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
            EditorGUI.PropertyField(position, property, label, true);

            if (property.objectReferenceValue == null)
            {
                return;
            }

            property.isExpanded = EditorGUI.Foldout(position, property.isExpanded, GUIContent.none);
            
            if (property.isExpanded)
            {
                if (!_editor)
                {
                    UnityEditor.Editor.CreateCachedEditor(property.objectReferenceValue, null, ref _editor);
                }
                
                _editor.OnInspectorGUI();
            }
        }
    }
}