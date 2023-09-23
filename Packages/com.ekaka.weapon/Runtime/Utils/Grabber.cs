using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using Inventory.Main;
using Inventory.Main.Slot;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Weapon.Utils
{
    [RequireComponent(typeof(InventoryController))]
    public class Grabber : MonoBehaviour
    {
        [Serializable]
        public struct GrabTarget
        {
            [field: SerializeField] public UsableSlotType SlotType { get; private set; }
            
            [field: SerializeField] public Transform Target { get; private set; }
        }
        
        [Serializable]
        public class Hand
        {
            [HideInInspector] public Transform Target;

            [field: SerializeField] public TwoBoneIKConstraint Constraint { get; set; }

            [Tooltip("Which slots use this hand")] [HideInInspector]
            public GenericDictionary<UsableSlotType, bool> Dependencies =
                GenericDictionary<UsableSlotType, bool>.ToGenericDictionary(Core.Utils.Utils.GetEnumValues<UsableSlotType>()
                    .ToDictionary(s => s, s => false));
        }

        [Tooltip("This is where items are placed in (re-parented to be grabbed)")] [HideInInspector]
        public GenericDictionary<UsableSlotType, Transform> Docks =
            GenericDictionary<UsableSlotType, Transform>.ToGenericDictionary(Core.Utils.Utils.GetEnumValues<UsableSlotType>()
                .ToDictionary(s => s, s => default(Transform)));

        [HideInInspector] public Hand[] Hands = { };

        public void Equip(UsableSlotType slotType)
        {
            foreach (Hand hand in Hands.Where(h => h.Dependencies[slotType])) hand.Target = null;
        }

        public void Equipped(UsableSlotType slotType, GrabTarget[] targets)
        {
            Hand[] hands = Hands.Where(h => h.Dependencies[slotType]).ToArray();

            if (hands.Length <= 0)
            {
                Debug.LogError($"No hands with {slotType} dependencies found");
                
                return;
            }

            if (targets != null && targets.Length > 0)
            {
                foreach (GrabTarget target in targets)
                {
                    Hand hand = hands.FirstOrDefault(h => h.Dependencies[target.SlotType]);

                    if (hand != null) hand.Target = target.Target;
                    
                    else Debug.LogError($"No hand with {target.SlotType} dependencies found");
                }
            }
            
            else Debug.LogError("NullOrEmpty targets");
        }
        
        public void UnEquip(UsableSlotType slotType)
        {
            foreach (Hand hand in Hands.Where(h => h.Dependencies[slotType])) hand.Target = null;
        }

        private void Update()
        {
            foreach (Hand hand in Hands)
            {
                if (hand.Target != null)
                {
                    hand.Constraint.weight = 1;

                    hand.Constraint.data.target.position = hand.Target.position;
                    hand.Constraint.data.target.rotation = hand.Target.rotation;
                }

                else
                    hand.Constraint.weight = 0;
            }
        }
    }
}