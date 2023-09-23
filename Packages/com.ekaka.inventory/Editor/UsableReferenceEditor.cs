using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using Editor.Core;
using Inventory.Main.Item;
using UnityEditor;
using UnityEngine;

namespace Inventory.Editor
{
    [CustomEditor(typeof(UsableReference), true)]
    public class UsableReferenceEditor : UnityEditor.Editor
    {
        private bool _overrideFoldout;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            
            SerializedProperty overrideProperty = serializedObject.FindProperty(nameof(UsableReference.ClipOverride));

            _overrideFoldout = EditorGUILayout.Foldout(_overrideFoldout,
                Utils.GetDisplayName(nameof(UsableReference.ClipOverride)));

            if (_overrideFoldout) BaseEditor.DrawClipOverride(overrideProperty);

            serializedObject.ApplyModifiedProperties();
        }
    }
}
