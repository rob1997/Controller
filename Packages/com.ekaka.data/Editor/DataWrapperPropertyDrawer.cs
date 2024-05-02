using Data.Main;
using Data.Persistence;
using Core.Editor;
using UnityEditor;
using UnityEngine;

namespace Data.Editor
{
    [CustomPropertyDrawer(typeof(DataWrapper<>), true)]
    public class DataWrapperPropertyDrawer : PropertyDrawer
    {
        private bool _foldout;
        
        private Rect _position;
        
        private const float DefaultPadding = 5f;
        
        private float SingleLineHeight => EditorGUIUtility.singleLineHeight + DefaultPadding;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            _position = position;
            
            DrawDataWrapper((dynamic) property.GetValue(), property);
            
            //deduct difference so _position.y can be equal to real height of property
            _position.y -= position.y;

            property.serializedObject.ApplyModifiedProperties();
        }

        private void DrawDataWrapper<TDataModel>(DataWrapper<TDataModel> data, SerializedProperty property) where TDataModel : IDataModel, new()
        {
            Object targetObject = property.serializedObject.targetObject;
            
            SerializedProperty idProperty = property.FindPropertyRelative(nameof(IDataWrapper.Id).GetPropertyName());
            //if id is null or empty create new file
            if (string.IsNullOrEmpty(idProperty.stringValue))
            {
                //reset dataWrapper
                data.ResetData();
                
                EditorUtility.SetDirty(targetObject);
            }
            
            SetDefaultRectHeight();
            
            _foldout = EditorGUI.Foldout(_position, _foldout, new GUIContent(property.displayName, property.tooltip));

            MoveRectDown();
            
            if (!_foldout)
            {
                return;
            }
            
            //id label
            SetRectHeight(EditorGUI.GetPropertyHeight(idProperty, true));
                
            EditorGUI.LabelField(_position, idProperty.stringValue);
            MoveRectDown();
                
            //generate button
            SetDefaultRectHeight();
                
            bool generateNewId = GUI.Button(_position, new GUIContent("Generate", "this will generate/re-generate new id and saved file and reset the DataModel, it will also delete any existing old files"));
            if (generateNewId)
            {
                //reset dataWrapper
                data.ResetData();
                
                EditorUtility.SetDirty(targetObject);
            }
            MoveRectDown();
                
            //save button
            SetDefaultRectHeight();
                
            bool saveData = GUI.Button(_position, new GUIContent("Save", "Save DataModel on file"));
            if (saveData)
            {
                //save dataModel
                data.SaveData();
                
                EditorUtility.SetDirty(targetObject);
            }
            MoveRectDown();
                
            //load button
            SetDefaultRectHeight();
                
            bool loadData = GUI.Button(_position, new GUIContent("Load", "Load DataModel from file"));
            if (loadData)
            {
                //load dataModel
                data.LoadData();
                
                EditorUtility.SetDirty(targetObject);
            }
                
            MoveRectDown();

            //update button
            //updates data with current data, useful for level editing
            SetDefaultRectHeight();
            
            if (targetObject is IStorable storable)
            {
                bool updateData = GUI.Button(_position, new GUIContent("Update", "Update DataModel from Storable"));
                
                if (updateData)
                {
                    storable.UpdateData();
                    
                    EditorUtility.SetDirty(targetObject);
                }
            }
            
            MoveRectDown();
            
            //draw DataModel //dynamic
            SerializedProperty dataModelProperty = property.FindPropertyRelative(nameof(data.DataModel).GetPropertyName());
                
            SetRectHeight(EditorGUI.GetPropertyHeight(dataModelProperty, true));
                
            EditorGUI.PropertyField(_position, dataModelProperty, new GUIContent(dataModelProperty.displayName, dataModelProperty.tooltip), true);
                
            MoveRectDown();
        }
        
        private void SetRectHeight(float height)
        {
            _position.height = height;
        }
        
        private void SetDefaultRectHeight()
        {
            SetRectHeight(SingleLineHeight);
        }
        
        private void MoveRectDown()
        {
            _position.y += _position.height;
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return _foldout ? _position.y : SingleLineHeight;
        }
    }
}
