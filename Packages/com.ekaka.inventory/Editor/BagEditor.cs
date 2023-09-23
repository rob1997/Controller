using Core.Utils;
using Inventory.Main;
using UnityEditor;
using UnityEngine;

namespace Inventory.Editor
{
    [CustomEditor(typeof(Bag))]
    public class BagEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            Bag bag = (Bag) target;

            if (bag == null) return;
        
            EditorGUILayout.Space(5f);

            int gearSlotCount = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent(Utils
                .GetDisplayName(nameof(bag.GearSlotCount)), "Total Gears Bag Slots"), bag.GearSlotCount), 0, int.MaxValue);
        
            bag.ResizeGearSlots(gearSlotCount);
            
            int supplementSlotCount = Mathf.Clamp(EditorGUILayout.IntField(new GUIContent(Utils
                .GetDisplayName(nameof(bag.SupplementSlotCount)), "Total Supplements Bag Slots"), bag.SupplementSlotCount), 0, int.MaxValue);
        
            bag.ResizeSupplementSlots(supplementSlotCount);
            
            float limit = Mathf.Clamp(EditorGUILayout.FloatField(new GUIContent(Utils
                .GetDisplayName(nameof(bag.Limit)), "Weight Limit of Bag"), bag.Limit), 0, float.MaxValue);
            
            bag.ResizeLimit(limit);
        }
    }
}
