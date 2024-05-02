using System;
using System.Collections;
using System.Collections.Generic;
using Core.Common;
using Inventory.Main.Item;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Inventory.Main.Slot
{
    public enum SlotState
    {
        //isEquipped = false
        UnEquipped,
        Equipping,
            
        //isEquipped = true
        Equipped,
        UnEquipping,
    }
    
    [Serializable]
    public abstract class Slot<T> where T : class, IGearAdapter
    {
        [SerializeField] protected T adapter;
        
        [field: SerializeField] public Transform EquipBone { get; set; }
        
        [field: SerializeField] public SlotState State { get; private set; }

        public IGear Gear { get; private set; }
        
        public bool IsEquipped => State == SlotState.Equipped;

        public T Adapter => adapter;
        
        public InventoryController controller;
        
        protected void Switch()
        {
            Switch(Gear);
        }
        
        //switch to null for unEquip
        public void Switch(IGear gear)
        {
            Gear = gear;

            if (!CanSwitch()) return;

            switch (State)
            {
                case SlotState.UnEquipped:
                    //already unequipped (avoid circular dependency on usable slots)
                    if (gear == null) return;
                    UnEquipped();
                    break;
                
                case SlotState.Equipped:
                    //already equipped with the same item (avoid circular dependency on usable slots)
                    if (gear != null && gear.Id == adapter?.Item?.Id) return;
                    Equipped();
                    break;
            }
        }
        
        protected abstract bool CanSwitch();
        
        protected void Equip()
        {
            //no item/adapter in slot
            //or
            //new item is being equipped
            if (adapter == null || adapter.Item.Id != Gear.Id)
            {
                //destroy old
                if (adapter?.Obj != null) adapter.Obj.Destroy();
                
                GameObject obj = Object.Instantiate(Gear.Reference.Prefab, EquipBone);
                
                //has no adapter on object
                //assign adapter too
                if (!obj.TryGetComponent(out adapter)) Debug.LogError($"Item adapter not Found on {Gear.Title} Prefab");
                
                adapter.Initialize(Gear, controller.Actor);

                RegisterEquippedCallback();
                
                obj.transform.LocalReset();
                
                RegisterUnEquippedCallback();
            }

            //re-equipping from adapter (same item)
            else
            {
                adapter.Obj.transform.LocalReset(EquipBone);

                //if it's start with item then we need to reassign events
                if (adapter.Equipped == null) RegisterEquippedCallback();
                
                if (adapter.UnEquipped == null) RegisterUnEquippedCallback();
            }
            
            State = SlotState.Equipping;
            
            controller.InvokeEquipInitialized(adapter.Gear);
            
            adapter.Equip();
        }

        private void RegisterEquippedCallback()
        {
            //assign adapter delegates
            adapter.Equipped += delegate
            {
                State = SlotState.Equipped;
                    
                adapter.EquippedCallback();

                Equipped();
            };
        }
        
        private void RegisterUnEquippedCallback()
        {
            //assign adapter delegates
            adapter.UnEquipped += delegate
            {
                State = SlotState.UnEquipped;
               
                adapter.UnEquippedCallback();
                
                UnEquipped();
            };
        }
        
        protected virtual void Equipped()
        {
            controller.InvokeEquipped(adapter.Gear);
            
            //if un-equipping or equipping a different item => un-equip current one
            if (Gear == null || Gear.Id != adapter.Item.Id)
            {
                UnEquip();
            }
        }

        protected void UnEquip()
        {
            State = SlotState.UnEquipping;
            
            controller.InvokeUnEquipInitialized(adapter.Gear);
            
            adapter.UnEquip();
        }

        protected virtual void UnEquipped()
        {
            controller.InvokeUnEquipped(adapter?.Gear);
        }

        public virtual void StartWith(T startWithAdapter)
        {
            if (startWithAdapter == null || startWithAdapter.Obj == null) return;
            
            //destroy old
            if (adapter?.Obj != null) adapter.Obj.Destroy();

            adapter = startWithAdapter;
            
            Gear = null;
        }
    }
}
