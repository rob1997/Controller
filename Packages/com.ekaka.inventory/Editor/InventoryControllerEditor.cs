using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using Editor.Core;
using Inventory.Main;
using Inventory.Main.Item;
using Inventory.Main.Slot;
using UnityEditor;
using UnityEngine;

namespace Inventory.Editor
{
    [CustomEditor(typeof(InventoryController), true)]
    public class InventoryControllerEditor : UnityEditor.Editor
    {
        private bool _usablesFoldout;

        private bool _wearablesFoldout;

        private Dictionary<UsableSlotType, bool> _dependencyFoldout;

        private InventoryController _controller;

        private void OnEnable()
        {
            _dependencyFoldout = Utils.GetEnumValues<UsableSlotType>().ToDictionary(u => u, u => false);
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            if (_controller == null) _controller = (InventoryController) target;

            if (_controller.Bag != null)
            {
                if (GUILayout.Button(new GUIContent(nameof(Bag), "Opens Bag Window")))
                {
                    if (!_controller.Bag.Initialized)
                    {
                        _controller.Bag.Initialize();
                    }

                    BagWindow.Initialize(_controller);
                }
            }

            DrawUsableDict();

            DrawWearableDict();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawUsableDict()
        {
            SerializedProperty usableDictProperty = serializedObject.FindProperty(InventoryController.UsableName);

            _usablesFoldout = EditorGUILayout.Foldout(_usablesFoldout, usableDictProperty.displayName);

            if (!_usablesFoldout) return;

            BaseEditor.DrawEnumDict<UsableSlotType, UsableSlot>(usableDictProperty, DrawUsableSlot);
        }

        private void DrawWearableDict()
        {
            SerializedProperty wearableDictProperty = serializedObject.FindProperty(InventoryController.WearableName);

            _wearablesFoldout = EditorGUILayout.Foldout(_wearablesFoldout, wearableDictProperty.displayName);

            if (!_wearablesFoldout) return;

            BaseEditor.DrawEnumDict<WearableSlotType, WearableSlot>(wearableDictProperty, DrawWearableSlot);
        }

        private void DrawUsableSlot(SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();

            var pair = (GenericDictionary<UsableSlotType, UsableSlot>.GenericPair) property.GetValue();

            UsableSlot slot = pair.Value;

            EditorGUI.BeginDisabledGroup(true);
            
            EditorGUILayout.LabelField($"State : {Utils.GetDisplayName(slot.State.ToString())}");
            
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.Space();
            
            if (slot.controller == null) slot.controller = _controller;

            slot.EquipBone = (Transform) EditorGUILayout.ObjectField(Utils.GetDisplayName(nameof(slot.EquipBone)),
                slot.EquipBone, typeof(Transform), true);

            slot.UnEquipBone = (Transform) EditorGUILayout.ObjectField(Utils.GetDisplayName(nameof(slot.UnEquipBone)),
                slot.UnEquipBone, typeof(Transform), true);

            EditorGUILayout.Space();

            _dependencyFoldout[pair.Key] = EditorGUILayout.Foldout(_dependencyFoldout[pair.Key],
                Utils.GetDisplayName(nameof(slot.Dependencies)));

            if (_dependencyFoldout[pair.Key])
            {
                UsableSlotType[] allSlots = Utils.GetEnumValues<UsableSlotType>().Where(e => e != pair.Key).ToArray();

                UsableSlotType[] dependencies = slot.Dependencies;

                foreach (var slotType in allSlots)
                {
                    bool dependent = dependencies.Contains(slotType);

                    if (EditorGUILayout.Toggle(Utils.GetDisplayName($"{slotType}"), dependent))
                    {
                        if (!dependent)
                        {
                            //Add dependency
                            slot.AddDependency(slotType);
                            //add counterpart dependency
                            _controller.Usables[slotType].AddDependency(pair.Key);
                        }
                    }

                    else
                    {
                        if (dependent)
                        {
                            //remove dependency
                            slot.RemoveDependency(slotType);
                            //remove counterpart dependency
                            _controller.Usables[slotType].RemoveDependency(pair.Key);
                        }
                    }
                }
            }

            EditorGUILayout.Space();

            DrawStartWithUsable(ref slot, pair.Key);

            if (slot.Adapter?.Obj != null)
            {
                if (GUILayout.Button(new GUIContent("Clear"))) slot.Adapter.Obj.Destroy();
            }

            pair.SetValue(slot);
            
            if (EditorGUI.EndChangeCheck()) property.SetValue(pair);
        }

        private void DrawStartWithUsable(ref UsableSlot slot, UsableSlotType key)
        {
            if (_controller.Bag == null)
            {
                GameObject obj = (GameObject) EditorGUILayout.ObjectField(new GUIContent("Start With"),
                    slot.Adapter?.Obj, typeof(GameObject), true);

                if (obj != null && obj != slot.Adapter?.Obj)
                {
                    //is prefab (not scene object)
                    bool isPrefab = obj.scene.rootCount == 0;

                    if (isPrefab) obj = Instantiate(obj);

                    //is scene object
                    if (obj.TryGetComponent(out IUsableAdapter adapter) &&
                        //check if slots are similar
                        ((UsableReference) adapter.Gear.Reference).SlotType == key)
                    {
                        //clone and initialize
                        adapter.StartWith(adapter.Item.Clone<IUsable>(), _controller.Actor);

                        slot.StartWith(adapter);
                    }

                    else if (isPrefab) obj.Destroy();
                }
            }

            else
            {
                int[] indexes = _controller.Bag.Gears.FindIndexes(g =>
                    g is IUsable && ((UsableReference) g.Reference).SlotType == key);

                string[] options = Array.ConvertAll(indexes, i => $"{i} : {_controller.Bag.Gears[i].Reference.Title}");

                int index = -1;

                index = EditorGUILayout.Popup(new GUIContent("Start With"), index, options);

                if (index != -1)
                {
                    IUsable usable = (IUsable) _controller.Bag.Gears[indexes[index]];

                    GameObject obj = Instantiate(usable.Reference.Prefab);

                    //is scene object
                    if (obj.TryGetComponent(out IUsableAdapter adapter))
                    {
                        //since it's from bag item is initialized with bag item
                        adapter.StartWith(usable, _controller.Actor);

                        slot.StartWith(adapter);
                    }

                    else
                        obj.Destroy();
                }

                EditorGUI.BeginDisabledGroup(true);

                EditorGUILayout.ObjectField(slot.Adapter?.Obj, typeof(GameObject), true);

                EditorGUI.EndDisabledGroup();
            }
        }

        private void DrawWearableSlot(SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();

            var pair = (GenericDictionary<WearableSlotType, WearableSlot>.GenericPair) property.GetValue();

            WearableSlot slot = pair.Value;

            if (slot.controller == null) slot.controller = _controller;

            slot.EquipBone = (Transform) EditorGUILayout.ObjectField(Utils.GetDisplayName(nameof(slot.EquipBone)),
                slot.EquipBone, typeof(Transform), true);

            pair.SetValue(slot);
            
            if (EditorGUI.EndChangeCheck()) property.SetValue(pair);
        }
    }
}