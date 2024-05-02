using System;
using System.IO;
using System.Linq;
using Data.SceneLink;
using Core.Editor;
using Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Data.Editor
{
    [CustomPropertyDrawer(typeof(CrossSceneReference<>))]
    public class CrossSceneReferencePropertyDrawer : PropertyDrawer
    {
        private int _selectedIndex;

        private Rect _position;

        private Component _component;

        private bool _foldout;

        //tried to load _component from all GlobalReferenceWrappers
        private bool _loadAttempted;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _position = position;

            _position.height = EditorGUIUtility.singleLineHeight;

            _foldout = EditorGUI.Foldout(_position, _foldout, label);

            MovePositionDown();

            if (_foldout)
            {
                DrawCrossReference((dynamic) property.GetValue(), property);
            }

            _position.y -= position.y;

            property.serializedObject.ApplyModifiedProperties();
        }

        private void DrawCrossReference<T>(CrossSceneReference<T> sceneReference, SerializedProperty property)
            where T : class
        {
            string path = GlobalReferenceWrapper.ReferenceFilePath;

            //create file if it doesn't exist
            if (!File.Exists(path))
            {
                GlobalReferenceWrapper.CreateFile();
            }

            string rawJson = File.ReadAllText(path);

            GlobalReference[] references = JsonConvert.DeserializeObject<GlobalReference[]>(rawJson);

            //make sure types match
            references = references.Where(r => typeof(T).IsAssignableFrom(r.Type)).ToArray();

            if (references.Length <= 0)
            {
                EditorGUI.HelpBox(_position, $"No {nameof(SceneLink)} with {typeof(T).Name} {nameof(Type)} found",
                    MessageType.Warning);
                //move down position rect
                MovePositionDown();

                return;
            }

            string[] referenceShortNames = Array.ConvertAll(references, r => $"{r.Id.Substring(0, 5)}-{r.Type.Name}");

            SerializedProperty idProperty = property.FindPropertyRelative(nameof(sceneReference.Id).GetPropertyName());

            string oldId = sceneReference.Id;

            //in case it's - 1 (id was empty/not found)
            _selectedIndex = Mathf.Max(0, Array.FindIndex(references, r => r.Id == oldId));

            _selectedIndex = EditorGUI.Popup(_position, _selectedIndex, referenceShortNames);
            //move down position rect
            MovePositionDown();

            string newId = references[_selectedIndex].Id;

            //changes made
            if (oldId != newId)
            {
                //find component again
                _component = null;

                _loadAttempted = false;
                //assign newId
                idProperty.stringValue = newId;
            }

            //Display Object reference if loaded
            if (_component == null && !_loadAttempted)
            {
                LoadComponent(idProperty.stringValue);
            }

            else
            {
                if (_component != null)
                {
                    EditorGUI.BeginDisabledGroup(true);

                    _component = (Component) EditorGUI.ObjectField(_position, _component, typeof(Component), true);

                    EditorGUI.EndDisabledGroup();
                }

                else
                {
                    EditorGUI.LabelField(_position, new GUIContent($"{nameof(SceneLink)} not loaded"));
                }

                MovePositionDown();
            }
        }

        private void LoadComponent(string id)
        {
            GlobalReferenceWrapper wrapper = Object.FindObjectsOfType<GlobalReferenceWrapper>()
                .FirstOrDefault(w => w.References.Any(r => r.Id == id));

            if (wrapper != null)
            {
                GlobalReference reference = wrapper.References.First(r => r.Id == id);

                _component = reference.Reference;
            }

            _loadAttempted = true;
        }

        private void MovePositionDown()
        {
            _position.y += _position.height;
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _foldout ? _position.y : _position.height;
        }
    }
}