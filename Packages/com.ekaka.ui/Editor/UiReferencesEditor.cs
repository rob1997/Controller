using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using Editor.Core;
using Ui.Main;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ui.Editor
{
    [CustomEditor(typeof(UiReferences))]
    public class UiReferencesEditor : UnityEditor.Editor
    {
        //foldout bools
        private bool _uiMenuReferenceFoldout;
        private bool _uiRegionTypesFoldout;
        private bool _uiLayerTypesFoldout;

        private bool _landingUiMenusFoldout;
        
        //popup/dropdown selected element
        private int _selectedUiRegionType;
        private int _selectedUiLayerType;

        //new added values
        private string _newUiMenuReference;
        private string _newUiRegionType;
        private string _newUiLayerReference;

        private UiReferences _uiReferences;
        
        public override void OnInspectorGUI()
        {
            _uiReferences = (UiReferences) target;
            
            //draw uiMenu references
            string uiMenuReferenceDisplayName = nameof(UiReferences.UiMenuReferences);

            _uiMenuReferenceFoldout = EditorGUILayout.Foldout(_uiMenuReferenceFoldout,
                new GUIContent(uiMenuReferenceDisplayName, "All Menu References"));

            SerializedProperty uiMenuReferencesProperty = serializedObject.FindProperty(uiMenuReferenceDisplayName);

            if (_uiMenuReferenceFoldout)
            {
                DrawUiMenuReferenceDict(uiMenuReferencesProperty, ref _newUiMenuReference);
            }

            EditorGUILayout.Space();

            //draw uiRegion types
            string uiRegionTypeDisplayName = nameof(UiReferences.UiRegionTypes);

            _uiRegionTypesFoldout = EditorGUILayout.Foldout(_uiRegionTypesFoldout,
                new GUIContent(uiRegionTypeDisplayName, "All Region Types"));

            if (_uiRegionTypesFoldout)
            {
                DrawUiTypeList(serializedObject.FindProperty(uiRegionTypeDisplayName), ref _selectedUiRegionType,
                    ref _newUiRegionType);
            }

            EditorGUILayout.Space();

            //draw uiLayer types
            string uiLayerTypesDisplayName = nameof(UiReferences.UiLayerTypes);

            _uiLayerTypesFoldout = EditorGUILayout.Foldout(_uiLayerTypesFoldout,
                new GUIContent(uiLayerTypesDisplayName, "All Layer Types"));

            if (_uiLayerTypesFoldout)
            {
                DrawUiTypeList(serializedObject.FindProperty(uiLayerTypesDisplayName), ref _selectedUiLayerType,
                    ref _newUiLayerReference);
            }

            EditorGUILayout.Space();

            //draw landingUiMenus
            _landingUiMenusFoldout = EditorGUILayout.Foldout(_landingUiMenusFoldout,
                new GUIContent(nameof(UiReferences.LandingUiMenus), "UiMenus that load when the game starts"));

            if (_landingUiMenusFoldout)
            {
                DrawLandingUiMenuList();
            }
            
            EditorGUILayout.Space();

            serializedObject.DrawInspectorExcept(new []{ BaseEditor.ScriptPropertyPath });
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawLandingUiMenuList()
        {
            //get all scene Names in build settings
            string[] sceneNames = Core.Utils.Utils.GetAllSceneNamesInBuildSettings();

            string[] uiMenuTypes = _uiReferences.UiMenuReferences.Keys.ToArray();
            
            //to set value (not to make change/apply modified property every frame)
            bool changesMade = false;

            SerializedProperty landingUiMenusProperty =
                serializedObject.FindProperty(nameof(UiReferences.LandingUiMenus));
            
            SerializedProperty pairListProperty = landingUiMenusProperty.FindPropertyRelative(BaseEditor.SerializedListName);

            List<GenericDictionary<int, string[]>.GenericPair> landingUiMenuList = (List<GenericDictionary<int, string[]>.GenericPair>) pairListProperty.GetValue();
            
            //remove all entries with incorrect keys/scene indexes
            int removed = landingUiMenuList.RemoveAll(l => l.Key >= sceneNames.Length || l.Key < 0);
            //record changes
            if (removed > 0) changesMade = true;
            
            for (int i = 0; i < sceneNames.Length; i++)
            {
                string sceneName = sceneNames[i];
                
                EditorGUILayout.BeginVertical(BaseEditor.Box);
                
                EditorGUILayout.LabelField(sceneName, GUILayout.MaxWidth(BaseEditor.LabelWidth));
                
                //remove all entries with incorrect values/uiMenuTypes
                foreach (var corruptedPair in landingUiMenuList.Where(l => l.Value.Any(t => !uiMenuTypes.Contains(t))))
                {
                    landingUiMenuList[corruptedPair.Key] = new GenericDictionary<int, string[]>.GenericPair(corruptedPair.Key, 
                        landingUiMenuList[corruptedPair.Key].Value.Where(l => uiMenuTypes.Contains(l)).ToArray());
                    
                    changesMade = true;
                }
                
                if (!landingUiMenuList.Exists(l => l.Key == i))
                {
                    landingUiMenuList.Add(new GenericDictionary<int, string[]>.GenericPair(i, new string[]{ }));
                    
                    changesMade = true;
                }
                
                foreach (string uiMenuType in uiMenuTypes)
                {
                    EditorGUILayout.BeginHorizontal();

                    bool isLandingUiMenuToggle = landingUiMenuList[i].Value.Contains(uiMenuType);

                    //check if changes were made in toggle
                    if (EditorGUILayout.Toggle(isLandingUiMenuToggle, GUILayout.MaxWidth(BaseEditor.SmallButtonWidth)) !=
                        isLandingUiMenuToggle)
                    {
                        changesMade = true;
                        //opposite changes made (removed/unchecked)
                        if (isLandingUiMenuToggle)
                        {
                            //remove and assign
                            landingUiMenuList[i] = new GenericDictionary<int, string[]>.GenericPair(i, landingUiMenuList[i]
                                .Value.Where(l => l != uiMenuType).ToArray());
                        }
                        //opposite changes made (added/checked)
                        else
                        {
                            //append and assign
                            landingUiMenuList[i] = new GenericDictionary<int, string[]>.GenericPair(i, landingUiMenuList[i]
                                .Value.Append(uiMenuType).ToArray());
                        }
                    }

                    EditorGUILayout.LabelField(uiMenuType, GUILayout.MaxWidth(BaseEditor.LabelWidth));

                    EditorGUILayout.EndHorizontal();
                }
                
                EditorGUILayout.EndVertical();
            }
            
            if (changesMade)
            {
                pairListProperty.SetValue(landingUiMenuList);
            }
        }

        //for UiMenus
        //ref parameter because it's fields cached for next frame
        private void DrawUiMenuReferenceDict(SerializedProperty dictProperty, ref string newValue)
        {
            GenericDictionary<string, AssetReference> references =
                (GenericDictionary<string, AssetReference>) dictProperty.GetValue();

            SerializedProperty pairListProperty = dictProperty.FindPropertyRelative(BaseEditor.SerializedListName);

            foreach (SerializedProperty property in pairListProperty)
            {
                EditorGUILayout.BeginHorizontal();

                SerializedProperty keyProperty = property.FindPropertyRelative(BaseEditor.KeyName);

                string key = keyProperty.stringValue;

                //draw key
                EditorGUILayout.LabelField(key, GUILayout.MaxWidth(BaseEditor.LabelWidth));

                SerializedProperty valueProperty = property.FindPropertyRelative(BaseEditor.ValueName);

                //draw value
                float cachedLabelWidth = EditorGUIUtility.labelWidth;
                //can't be zero, just resets to original width
                EditorGUIUtility.labelWidth = .01f;

                EditorGUILayout.PropertyField(valueProperty);

                //revert label width value
                EditorGUIUtility.labelWidth = cachedLabelWidth;

                //remove from dict MAKE SURE THERE'S NO REFERENCES
                if (GUILayout.Button(new GUIContent(" - ", "Remove Element - MAKE SURE THERE'S NO REFERENCES"),
                    GUILayout.MaxWidth(BaseEditor.SmallButtonWidth)))
                {
                    references.Remove(key);

                    dictProperty.SetValue(references);
                    //show immediate change
                    serializedObject.Update();
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.BeginHorizontal();

            newValue = EditorGUILayout.TextField(newValue, GUILayout.MaxWidth(BaseEditor.LabelWidth));

            if (!string.IsNullOrEmpty(newValue))
            {
                //check for duplicates before drawing add button
                if (references.ContainsKey(newValue))
                {
                    EditorGUILayout.HelpBox($"{newValue} Menu Type already exists", MessageType.Warning);
                }

                else
                {
                    //add to dict
                    if (GUILayout.Button(new GUIContent(" + ", "Add Element"),
                        GUILayout.MaxWidth(BaseEditor.SmallButtonWidth)))
                    {
                        references.Add(newValue, null);

                        newValue = string.Empty;
                        //so the above line/string.Empty is reflected
                        GUI.FocusControl(null);

                        dictProperty.SetValue(references);
                        //show immediate change
                        serializedObject.Update();
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }

        //for regions and layers
        //ref parameter because it's fields cached for next frame
        private void DrawUiTypeList(SerializedProperty arrayProperty, ref int selectedUiTypeIndex, ref string newValue)
        {
            string[] uiTypes = (string[]) arrayProperty.GetValue();

            EditorGUILayout.BeginHorizontal();
            //dropdown instead of list to save code and also space
            selectedUiTypeIndex =
                EditorGUILayout.Popup(selectedUiTypeIndex, uiTypes, GUILayout.MaxWidth(BaseEditor.LabelWidth));
            //make sure there's elements to remove before drawing remove button - remove from dict MAKE SURE THERE'S NO REFERENCES
            if (uiTypes.Length != 0 &&
                GUILayout.Button(new GUIContent(" - ", "Remove Element - MAKE SURE THERE'S NO REFERENCES"),
                    GUILayout.MaxWidth(BaseEditor.SmallButtonWidth)))
            {
                string selectedType = uiTypes[selectedUiTypeIndex];

                uiTypes = uiTypes.Where(t => t != selectedType).ToArray();

                arrayProperty.SetValue(uiTypes);

                selectedUiTypeIndex = Mathf.Clamp(selectedUiTypeIndex, 0, uiTypes.Length - 1);
            }

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            //add to array
            newValue = EditorGUILayout.TextField(newValue, GUILayout.MaxWidth(BaseEditor.LabelWidth));

            if (!string.IsNullOrEmpty(newValue))
            {
                //check for duplicates before drawing add button
                if (uiTypes.Contains(newValue))
                {
                    EditorGUILayout.HelpBox($"{newValue} Type already exists", MessageType.Warning);
                }

                else
                {
                    if (GUILayout.Button(new GUIContent(" + ", "Add Element"),
                        GUILayout.MaxWidth(BaseEditor.SmallButtonWidth)))
                    {
                        uiTypes = uiTypes.Append(newValue).ToArray();

                        newValue = string.Empty;
                        //so the above line/string.Empty is reflected
                        GUI.FocusControl(null);

                        arrayProperty.SetValue(uiTypes);
                        //set dropdown pointer to last element or last added element
                        selectedUiTypeIndex = uiTypes.Length - 1;
                    }
                }
            }

            EditorGUILayout.EndHorizontal();
        }
    }
}