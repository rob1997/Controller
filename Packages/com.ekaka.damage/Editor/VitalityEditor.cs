using Damage.Main;
using Core.Editor;
using UnityEditor;

namespace Damage.Editor
{
    [CustomEditor(typeof(Vitality))]
    public class VitalityEditor : UnityEditor.Editor
    {
        private bool _foldOut;
        
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        
            SerializedProperty dictProperty = serializedObject.FindProperty(nameof(Vitality.Resistance));
            
            DrawResistanceDict(dictProperty);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawResistanceDict(SerializedProperty dictProperty)
        {
            _foldOut = EditorGUILayout.Foldout(_foldOut, dictProperty.displayName);

            if (!_foldOut) return;
            
            BaseEditor.DrawEnumDict<DamageType, Resistance>(dictProperty, DrawValue);
        }
        
        private void DrawValue(SerializedProperty property)
        {
            SerializedProperty valueProperty = property.FindPropertyRelative(BaseEditor.ValueName);

            EditorGUILayout.PropertyField(valueProperty.FindPropertyRelative(nameof(Resistance.Invulnerable).GetPropertyName()));
            EditorGUILayout.PropertyField(valueProperty.FindPropertyRelative(nameof(Resistance.Value).GetPropertyName()));
        }
    }
}
