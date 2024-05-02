using System;
using System.Linq;
using Character.Main;
using Core.Common;
using UnityEngine;
using Inventory.Main.Item;
using Inventory.Main.Slot;

namespace Inventory.Main
{
    public abstract class InventoryController : Controller
    {
        //When UnEquip is Initialized
        #region UnEquipInitialized

        public delegate void UnEquipInitialized(IGear gear);

        public event UnEquipInitialized OnUnEquipInitialized;

        public void InvokeUnEquipInitialized(IGear gear)
        {
            OnUnEquipInitialized?.Invoke(gear);
        }

        #endregion
        
        //When Equip is Initialized
        #region EquipInitialized

        public delegate void EquipInitialized(IGear gear);

        public event EquipInitialized OnEquipInitialized;

        public void InvokeEquipInitialized(IGear gear)
        {
            OnEquipInitialized?.Invoke(gear);
        }

        #endregion
        
        //When Equip is Completed i.e. after animations, logic...
        #region Equipped

        public delegate void Equipped(IGear gear);

        public event Equipped OnEquipped;

        public void InvokeEquipped(IGear gear)
        {
            OnEquipped?.Invoke(gear);
        }

        #endregion

        //When UnEquip is Completed i.e. after animations, logic...
        #region UnEquipped

        public delegate void UnEquipped(IGear gear);

        public event UnEquipped OnUnEquipped;

        public void InvokeUnEquipped(IGear gear)
        {
            OnUnEquipped?.Invoke(gear);
        }

        #endregion
    
        [SerializeField] protected float interactRadius = 1.5f;
        
        [HideInInspector] public GenericDictionary<UsableSlotType, UsableSlot> Usables = GenericDictionary<UsableSlotType, UsableSlot>
            .ToGenericDictionary(Utils.GetEnumValues<UsableSlotType>().ToDictionary(s => s, s => new UsableSlot()));
        
        [HideInInspector] public GenericDictionary<WearableSlotType, WearableSlot> Wearables = GenericDictionary<WearableSlotType, WearableSlot>
            .ToGenericDictionary(Utils.GetEnumValues<WearableSlotType>().ToDictionary(s => s, s => new WearableSlot()));

        [field: SerializeField] public Bag Bag { get; protected set; } = new Bag();
        
        private Transform _characterTransform;

        //height of character from the waist
        private float _waistLine;

#if UNITY_EDITOR
        //used for editor scripting / finding private properties
        public const string UsableName = nameof(Usables);
        
        public const string WearableName = nameof(Wearables);
#endif
        
        public override void Initialize(Actor actor)
        {
            base.Initialize(actor);

            // TODO initialize from file or persistent source
            if (!Bag.Initialized)
            {
                Bag.Initialize();
            }
            
            _characterTransform = actor.transform;

            try
            {
                _waistLine = actor.CharacterController.height / 2f;
            }
            
            catch (Exception )
            {
                _waistLine = 1f;
            }

            actor.OnEventDispatched += (label, args) =>
            {
                switch (label)
                {
                    case nameof(Actor.Equipped):
                        Usables[(UsableSlotType) (int) args[0]].Adapter?.Equipped.Invoke();
                        break;
                    case nameof(Actor.UnEquipped):
                        Usables[(UsableSlotType) (int) args[0]].Adapter?.UnEquipped.Invoke();
                        break;
                }
            };
        }
        
        #region Equip

        public void Equip(int index)
        {
            IGear gear = Bag.Gears[index];

            if (gear == null)
            {
                Debug.LogError($"Gear Empty at index {index}");
                
                return;
            }
            
            switch (gear)
            {
                case IUsable usable:
                    EquipUsable(usable);    
                    break;
                
                case IWearable wearable:
                    EquipWearable(wearable);
                    break;
            }
        }
        
        private void EquipUsable(IUsable usable)
        {
            UsableSlot slot = Usables[usable.SlotType];
            
            slot.Switch(usable);
        }
        
        //used for Equipping characters that don't have bags like NPCs
        protected void EquipUsableSlot(UsableSlotType usableSlotType)
        {
            UsableSlot slot = Usables[usableSlotType];
            
            slot.Switch(slot.Adapter?.Gear);
        }
        
        private void EquipWearable(IWearable wearable)
        {
            WearableSlot slot = Wearables[wearable.SlotType];
            
            slot.Switch(wearable);
        }

        #endregion

        #region UnEquip

        public void UnEquip(int index)
        {
            IGear gear = Bag.Gears[index];

            if (gear == null)
            {
                Debug.LogError($"Gear Empty at index {index}");
                
                return;
            }
            
            switch (gear)
            {
                case IUsable usable:
                    UnEquipUsable(usable);
                    break;
                
                case IWearable wearable:
                    UnEquipWearable(wearable);
                    break;
            }
        }
        
        private void UnEquipUsable(IUsable usable)
        {
            UsableSlot slot = Usables[usable.SlotType];

            //make sure we're unEquipping the same item
            if (slot.Adapter?.Item?.Id != usable.Id)
            {
                Debug.LogError($"{usable.Title} not Equipped on {usable.SlotType}");
                
                return;
            }
            
            //unEquip
            slot.Switch(null);
        }
        
        private void UnEquipWearable(IWearable wearable)
        {
            WearableSlot slot = Wearables[wearable.SlotType];

            //make sure we're unEquipping the same item
            if (slot.Adapter?.Item?.Id != wearable.Id)
            {
                Debug.LogError($"{wearable.Title} not Equipped on {wearable.SlotType}");
                
                return;
            }
            
            //unEquip
            slot.Switch(null);
        }

        public void UnEquipUsableSlot(UsableSlotType slotType)
        {
            UsableSlot slot = Usables[slotType];
            
            slot.Switch(null);
        }
        
        public void UnEquipWearableSlot(WearableSlotType slotType)
        {
            WearableSlot slot = Wearables[slotType];
            
            slot.Switch(null);
        }

        public void UnEquipAll()
        {
            UnEquipAllUsables();
            UnEquipAllWearables();
        }
        
        public void UnEquipAllUsables()
        {
            foreach (var slot in Usables.Values)
            {
                slot.Switch(null);
            }
        }
        
        public void UnEquipAllWearables()
        {
            foreach (var slot in Wearables.Values)
            {
                slot.Switch(null);
            }
        }
        
        #endregion
        
        #region Drop Item

        public void DropGear(int index)
        {
            IGear gear = Bag.Gears[index];

            if (gear == null)
            {
                Debug.LogError($"Can't drop Item, Slot {index} Empty");
                
                return;
            }

            GameObject obj = Instantiate(gear.Reference.Prefab, GetDropPosition(), Quaternion.identity);

            if (obj.TryGetComponent(out IItemAdapter adapter))
            {
                adapter.Initialize(gear, null);
                
                Bag.RemoveGear(index);
            }

            else Debug.LogError("Can't Initialize, Adapter not found on dropped Object");
        }
        
        public void DropSupplement(int index, int count)
        {
            ISupplement supplement = Bag.Supplements[index];

            if (supplement == null)
            {
                Debug.LogError($"Can't drop Item, Slot {index} Empty");
                
                return;
            }

            GameObject obj = Instantiate(supplement.Reference.Prefab, GetDropPosition(), Quaternion.identity);

            //make a copy for the dropped supplement
            ISupplement supplementCopy = supplement.Clone<ISupplement>();
            
            supplement.Remove(count);

            if (supplement.Count == 0)
            {
                Bag.RemoveSupplement(index);
            }
            
            supplementCopy.Resize(Mathf.Clamp(count, 0, supplementCopy.Count));
            
            if (obj.TryGetComponent(out IItemAdapter adapter))
            {
                adapter.Initialize(supplementCopy, null);
            }
            
            else Debug.LogError("Can't Initialize, Adapter not found on dropped Object");
        }

        private Vector3 GetDropPosition()
        {
            Vector3 position = _characterTransform.position + _characterTransform.forward;

            position += _characterTransform.right * UnityEngine.Random.Range(- 1f, 1f);
            
            position.y = _waistLine;

            return position;
        }

        #endregion

        #region Use | Stop

        public bool TryUse(UsableSlotType slotType, UsageType usageType = UsageType.Primary)
        {
            UsableSlot slot = Usables[slotType];

            if (!slot.IsEquipped) return false;

            if (slot.Adapter?.Obj == null || !slot.Adapter.CanUse[usageType])
            {
                return false;
            }

            slot.Adapter.Use(usageType);

            return true;
        }
        
        public bool TryStop(UsableSlotType slotType, UsageType usageType = UsageType.Primary)
        {
            UsableSlot slot = Usables[slotType];

            if (!slot.IsEquipped) return false;

            if (slot.Adapter == null || slot.Adapter.CanUse[usageType])
            {
                return false;
            }

            slot.Adapter.Stop(usageType);

            return true;
        }

        #endregion
    }
}
