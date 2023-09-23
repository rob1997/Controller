using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using Editor.Core;
using Inventory.Main.Slot;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using Weapon.Utils;

namespace Editor.Weapon
{
    [CustomEditor(typeof(Grabber))]
    public class GrabberEditor : UnityEditor.Editor
    {
        private bool _docksFoldout;

        private bool _handsFoldout;

        public override void OnInspectorGUI()
        {
            Grabber grabber = (Grabber) target;

            if (grabber == null) return;

            _docksFoldout = EditorGUILayout.Foldout(_docksFoldout,
                new GUIContent(Utils.GetDisplayName(nameof(grabber.Docks)),
                    "This is where items are placed in (re-parented to be grabbed)"));

            if (_docksFoldout)
                BaseEditor.DrawEnumDict<UsableSlotType, Transform>(serializedObject.FindProperty(nameof(grabber.Docks)), DrawTransform);

            _handsFoldout = EditorGUILayout.Foldout(_handsFoldout, Utils.GetDisplayName(nameof(grabber.Hands)));

            if (_handsFoldout) DrawHands(serializedObject.FindProperty(nameof(grabber.Hands)));
        }

        private void DrawTransform(SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();
            
            while (property.Next(true))
            {
                if (property.name == BaseEditor.ValueName)
                {
                    EditorGUILayout.PropertyField(property);
                    
                    break;
                }
            }

            if (EditorGUI.EndChangeCheck()) serializedObject.ApplyModifiedProperties();
        }

        private void DrawHands(SerializedProperty handsProperty)
        {
            AddHand(handsProperty);

            for (int i = 0; i < handsProperty.arraySize; i++)
            {
                SerializedProperty handProperty = handsProperty.GetArrayElementAtIndex(i);

                Grabber.Hand hand = (Grabber.Hand) handProperty.GetValue();

                EditorGUILayout.BeginVertical("box");

                EditorGUILayout.BeginHorizontal();

                hand.Constraint = (TwoBoneIKConstraint) EditorGUILayout.ObjectField(
                    Utils.GetDisplayName(nameof(hand.Constraint)), hand.Constraint, typeof(TwoBoneIKConstraint), true);

                if (GUILayout.Button(new GUIContent("-", "Remove Hand"),
                    GUILayout.MaxWidth(BaseEditor.SmallButtonWidth)))
                {
                    handsProperty.DeleteArrayElementAtIndex(i);

                    serializedObject.ApplyModifiedProperties();

                    continue;
                }

                EditorGUILayout.EndHorizontal();

                BaseEditor.DrawEnumDict<UsableSlotType, bool>(handProperty
                    .FindPropertyRelative(nameof(Grabber.Hand.Dependencies)), DrawToggle);

                EditorGUILayout.EndVertical();
            }
        }

        private void AddHand(SerializedProperty handsProperty)
        {
            if (GUILayout.Button(new GUIContent("+", "Add Hand"), GUILayout.MaxWidth(BaseEditor.SmallButtonWidth)))
            {
                Grabber.Hand[] hands = (Grabber.Hand[]) handsProperty.GetValue();

                if (hands == null) hands = new Grabber.Hand[] { };

                handsProperty.SetValue(hands.Append(new Grabber.Hand()).ToArray());
            }
        }

        private void DrawToggle(SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();

            var pair = (GenericDictionary<UsableSlotType, bool>.GenericPair) property.GetValue();

            pair.Value = EditorGUILayout.Toggle(pair.Value);

            if (EditorGUI.EndChangeCheck()) property.SetValue(pair);
        }
    }
}