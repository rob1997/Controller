using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using Inventory.Main.Item;
using Inventory.Main.Slot;
using UnityEngine;

namespace Inventory.Main.Slot
{
    public enum WearableSlotType
    {
        Head,
        UpperBody,
    }
    
    [Serializable]
    public class WearableSlot : Slot<IWearableAdapter>
    {
        protected override bool CanSwitch()
        {
            return true;
        }

        protected override void UnEquipped()
        {
            base.UnEquipped();
            
            //make sure it's not an already empty slot
            if (adapter?.Obj != null) adapter.Obj.Destroy();

            adapter = null;
            
            if (Gear == null)
            {
                return;
            }
                    
            Equip();
        }

        public override void StartWith(IWearableAdapter startWithAdapter)
        {
            base.StartWith(startWithAdapter);
            
            adapter.Obj.transform.LocalReset(EquipBone);
        }
    }
}
