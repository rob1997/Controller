using System;
using Data.GlobalReference;
using Editor.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Data.Editor
{
    [CustomEditor(typeof(GlobalReferenceWrapper))]
    public class GlobalReferenceWrapperEditor : UnityEditor.Editor
    {
        private Component _newComponent;

        public override void OnInspectorGUI()
        {
            GlobalReferenceWrapper wrapper = (GlobalReferenceWrapper) target;

            if (wrapper == null)
            {
                return;
            }

            GlobalReference.GlobalReference[] references = wrapper.References;

            for (int i = 0; i < references.Length; i++)
            {
                GlobalReference.GlobalReference reference = references[i];

                EditorGUILayout.LabelField(new GUIContent($"Id - {reference.Id}", "unique GUiD reference Id"));

                EditorGUILayout.BeginHorizontal();

                Object obj = EditorGUILayout.ObjectField(
                    new GUIContent(nameof(GlobalReference.GlobalReference.Reference), "Referenced Unity Object"), reference.Reference,
                    reference.Type, true);

                if (GUILayout.Button(new GUIContent("-", "Remove Reference"),
                    GUILayout.MaxWidth(BaseEditor.SmallButtonWidth)))
                {
                    if (wrapper.RemoveReference(reference.Id))
                    {
                        //so changes can be saved
                        EditorUtility.SetDirty(target);
                    }
                }

                EditorGUILayout.EndHorizontal();

                if (obj == null)
                {
                    Debug.LogWarning($"{nameof(GlobalReference.GlobalReference.Reference)} can't be null or empty!");
                }

                else if (obj != reference.Reference)
                {
                    reference.SetReference((Component) obj);
                    //since reference is struct/value type we have to re-assign it
                    references[i] = reference;
                    //so changes can be saved
                    EditorUtility.SetDirty(target);
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Add New Reference");

            EditorGUILayout.BeginHorizontal();

            _newComponent = (Component) EditorGUILayout.ObjectField(_newComponent, typeof(Component), true);

            if (_newComponent != null)
            {
                if (GUILayout.Button(new GUIContent("+", "Add new Reference"),
                    GUILayout.MaxWidth(BaseEditor.SmallButtonWidth)))
                {
                    if (wrapper.AddReference(new GlobalReference.GlobalReference(Guid.NewGuid().ToString(), _newComponent)))
                    {
                        //so changes can be saved
                        EditorUtility.SetDirty(target);
                    }

                    _newComponent = null;
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}