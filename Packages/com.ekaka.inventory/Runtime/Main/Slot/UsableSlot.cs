using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using Inventory.Main.Item;
using UnityEngine;

namespace Inventory.Main.Slot
{
    public enum UsableSlotType
    {
        LeftHand = 0,
        RightHand = 1,
        TwoHand = 2,
    }
    
    [Serializable]
    public class UsableSlot : Slot<IUsableAdapter>
    {
        #region Animator Hashes
        
        public static readonly int EquipLeftHandHash = Animator.StringToHash($"Equip{UsableSlotType.LeftHand}");
        public static readonly int UnEquipLeftHandHash = Animator.StringToHash($"UnEquip{UsableSlotType.LeftHand}");
        
        public static readonly int EquipRightHandHash = Animator.StringToHash($"Equip{UsableSlotType.RightHand}");
        public static readonly int UnEquipRightHandHash = Animator.StringToHash($"UnEquip{UsableSlotType.RightHand}");
        
        public static readonly int EquipTwoHandHash = Animator.StringToHash($"Equip{UsableSlotType.TwoHand}");
        public static readonly int UnEquipTwoHandHash = Animator.StringToHash($"UnEquip{UsableSlotType.TwoHand}");

        #endregion
        
        //slots that need to be unEquipped for this to be equipped,
        //twoHanded is a dependency for both right/left hand,
        //right and left hand are dependencies for twoHanded
        //beware of circular dependency
        [field: SerializeField] public UsableSlotType[] Dependencies { get; private set; }
        
        [field: SerializeField] public Transform UnEquipBone { get; set; }
        
        protected override bool CanSwitch()
        {
            bool dependent = CheckDependency();

            //if dependent wait until dependencies are unequipped
            return !dependent;
        }

        private bool CheckDependency()
        {
            bool dependent = false;
            
            if (Dependencies != null)
            {   
                foreach (var dependency in Dependencies)
                {
                    UsableSlot slot = controller.Usables[dependency];
                    
                    if (slot.State != SlotState.UnEquipped)
                    {
                        slot.Switch(null);

                        dependent = true;
                    }
                }
            }
            
            //if dependent wait until dependencies are unequipped
            return dependent;
        }
        
        protected override void UnEquipped()
        {
            base.UnEquipped();
            
            //make sure it's not an already empty slot
            if (adapter?.Obj != null)
            {
                adapter.Obj.transform.SetParent(UnEquipBone);
                
                adapter.Obj.transform.localPosition = adapter.Holster.localPosition;
                adapter.Obj.transform.localRotation = adapter.Holster.localRotation;
            }
            
            if (Gear == null)
            {
                if (Dependencies != null)
                {
                    foreach (var dependency in Dependencies)
                    {
                        controller.Usables[dependency].Switch();
                    }
                }
                
                return;
            }
            
            Equip();
        }

        public override void StartWith(IUsableAdapter startWithAdapter)
        {
            base.StartWith(startWithAdapter);
            
            adapter.Obj.transform.LocalReset(UnEquipBone);
        }

        public void AddDependency(UsableSlotType slotType)
        {
            if (Dependencies == null) Dependencies = new UsableSlotType[]{};

            Dependencies = Dependencies.Append(slotType).ToArray();
        }
        
        public void RemoveDependency(UsableSlotType slotType)
        {
            if (Dependencies == null)
            {
                Dependencies = new UsableSlotType[]{};
                return;
            }

            Dependencies = Dependencies.Where(d => d != slotType).ToArray();
        }
    }
}
