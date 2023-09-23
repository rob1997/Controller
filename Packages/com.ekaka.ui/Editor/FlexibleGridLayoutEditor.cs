using System.Collections;
using System.Collections.Generic;
using Ui.Utils;
using UnityEditor;
using UnityEngine;

namespace Ui.Editor
{
    [CustomEditor(typeof(FlexibleGridLayout))]
    public class FlexibleGridLayoutEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            FlexibleGridLayout flexibleGridLayout = (FlexibleGridLayout) target;
            if (flexibleGridLayout == null) return;

            SerializedProperty paddingProperty = serializedObject.FindProperty(nameof(flexibleGridLayout.padding));
            EditorGUILayout.PropertyField(paddingProperty);

            SerializedProperty fitTypeProperty = serializedObject.FindProperty(nameof(flexibleGridLayout.fitType));
            EditorGUILayout.PropertyField(fitTypeProperty);

            EditorGUILayout.Space();
            
            EditorGUI.BeginDisabledGroup(fitTypeProperty.enumValueIndex != (int) FlexibleGridLayout.FitType.FixedRows);

            SerializedProperty rowsProperty = serializedObject.FindProperty(nameof(flexibleGridLayout.rows));
            rowsProperty.intValue = EditorGUILayout.IntField(nameof(flexibleGridLayout.rows),
                rowsProperty.intValue <= 0 ? 1 : rowsProperty.intValue);

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(fitTypeProperty.enumValueIndex != (int) FlexibleGridLayout.FitType.FixedColumns);

            SerializedProperty columnsProperty = serializedObject.FindProperty(nameof(flexibleGridLayout.columns));
            columnsProperty.intValue = EditorGUILayout.IntField(nameof(flexibleGridLayout.columns),
                columnsProperty.intValue <= 0 ? 1 : columnsProperty.intValue);

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(flexibleGridLayout.spacing)));

            EditorGUILayout.Space();
            
            SerializedProperty fitXProperty = serializedObject.FindProperty(nameof(flexibleGridLayout.fitX));
            SerializedProperty fitYProperty = serializedObject.FindProperty(nameof(flexibleGridLayout.fitY));

            EditorGUILayout.PropertyField(fitXProperty);
            EditorGUILayout.PropertyField(fitYProperty);
            
            SerializedProperty anchorFitXProperty = serializedObject.FindProperty(nameof(flexibleGridLayout.anchorFitX));
            SerializedProperty anchorFitYProperty = serializedObject.FindProperty(nameof(flexibleGridLayout.anchorFitY));
            
            EditorGUILayout.Space();
            
            EditorGUI.BeginDisabledGroup(fitXProperty.boolValue || anchorFitXProperty.boolValue);

            SerializedProperty cellWidthProperty = serializedObject.FindProperty(nameof(flexibleGridLayout.cellSize))
                .FindPropertyRelative("x");
            cellWidthProperty.floatValue = EditorGUILayout.FloatField("Cell Width", cellWidthProperty.floatValue);

            EditorGUI.EndDisabledGroup();

            EditorGUI.BeginDisabledGroup(fitYProperty.boolValue || anchorFitYProperty.boolValue);

            SerializedProperty cellHeightProperty = serializedObject.FindProperty(nameof(flexibleGridLayout.cellSize))
                .FindPropertyRelative("y");
            cellHeightProperty.floatValue = EditorGUILayout.FloatField("Cell Height", cellHeightProperty.floatValue);

            EditorGUI.EndDisabledGroup();

            EditorGUILayout.Space();
            
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(flexibleGridLayout.centerX)));
            EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(flexibleGridLayout.centerY)));

            EditorGUILayout.Space();

            EditorGUI.BeginDisabledGroup(fitXProperty.boolValue);
            
            EditorGUILayout.PropertyField(anchorFitXProperty);
            //not disabled and anchorFit is true
            if (!fitXProperty.boolValue && anchorFitXProperty.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(FlexibleGridLayout.anchorRectX)));
                
                EditorGUILayout.Space();
            }
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(fitYProperty.boolValue);
            
            EditorGUILayout.PropertyField(anchorFitYProperty);
            //not disabled and anchorFit is true
            if (!fitYProperty.boolValue && anchorFitYProperty.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(FlexibleGridLayout.anchorRectY)));
                
                EditorGUILayout.Space();
            }
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(fitXProperty.boolValue || !anchorFitXProperty.boolValue);

            SerializedProperty anchorFitCellWidthProperty = serializedObject.FindProperty(nameof(flexibleGridLayout.anchorCellSize))
                .FindPropertyRelative("x");
            
            anchorFitCellWidthProperty.floatValue = EditorGUILayout.FloatField("Anchor Cell Width", anchorFitCellWidthProperty.floatValue);

            EditorGUI.EndDisabledGroup();
            
            EditorGUI.BeginDisabledGroup(fitYProperty.boolValue || !anchorFitYProperty.boolValue);

            SerializedProperty anchorFitCellHeightProperty = serializedObject.FindProperty(nameof(flexibleGridLayout.anchorCellSize))
                .FindPropertyRelative("y");
            
            anchorFitCellHeightProperty.floatValue = EditorGUILayout.FloatField("Anchor Cell Height", anchorFitCellHeightProperty.floatValue);

            EditorGUI.EndDisabledGroup();
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}