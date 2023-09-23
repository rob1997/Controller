using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class GearAdapter<TItem, TReference> : ItemAdapter<TItem, TReference>, IGearAdapter
        where TItem : Gear<TReference> where TReference : GearReference
    {
        public bool IsEquipped { get; protected set; }
        
        public Equipped Equipped { get; set; }
        
        public UnEquipped UnEquipped { get; set; }

        public IGear Gear => item;
        
        public override void Pick(bool picked, string message)
        {
            if (picked)
            {
                Debug.Log(message);
            
                Destroy(gameObject);
            }

            else
            {
                Debug.LogWarning(message);
            }
        }

        public abstract void Equip();

        public virtual void UnEquip()
        {
            IsEquipped = false;
        }

        public virtual void EquippedCallback()
        {
            IsEquipped = true;
        }
        
        public abstract void UnEquippedCallback();
    }
}