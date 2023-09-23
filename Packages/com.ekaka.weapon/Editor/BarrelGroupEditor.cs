using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Weapon.Main;

namespace Editor.Weapon
{
    [CustomEditor(typeof(BarrelGroup))]
    public class BarrelGroupEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            BarrelGroup group = (BarrelGroup) target;

            if (group == null || !group.drawGizmo) return;
            
            if (GUILayout.Button(new GUIContent("Add Snapshot", "Add a shot snapshot to be used later")))
            {
                group.TakeSnapshot();
            }
        
            if (GUILayout.Button(new GUIContent("Randomize", "Randomizes local position of barrel points")))
            {
                Transform transform = group.transform;

                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).localPosition = new Vector3(GetRandom(1f), GetRandom(1f), 0);
                }
            }
        }

        private float GetRandom(float dimension)
        {
            dimension /= 2f;
        
            return Random.Range(- dimension, dimension);
        }
    }
}
