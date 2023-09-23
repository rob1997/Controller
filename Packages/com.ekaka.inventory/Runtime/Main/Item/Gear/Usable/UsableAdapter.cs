using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using Inventory.Main.Slot;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class UsableAdapter<TItem, TReference> : GearAdapter<TItem, TReference>, IUsableAdapter
        where TItem : Usable<TReference> where TReference : UsableReference
    {
        [field: SerializeField] public Transform Holster { get; protected set; }
        
        public Dictionary<UsageType, bool> CanUse { get; protected set; } =
            Utils.GetEnumValues<UsageType>().ToDictionary(u => u, u => false);
        
        /// <summary>
        /// add usages on <see cref="CharacterReady"/>
        /// </summary>
        private readonly Dictionary<UsageType, Action> _usages = Utils
            .GetEnumValues<UsageType>().ToDictionary(u => u, u => default(Action));

        private readonly Dictionary<UsageType, Action> _stoppages = Utils
            .GetEnumValues<UsageType>().ToDictionary(u => u, u => default(Action));

        private Animator _animator;

        protected override void CharacterReady()
        {
            _animator = Actor.Animator;

            transform.SetLocalPositionAndRotation(Holster.localPosition, Holster.localRotation);
        }

        public override void Equip()
        {
            SetUsable(false);
            
            _animator.OverrideClips(Reference.ClipOverride);

            switch (Reference.SlotType)
            {
                case UsableSlotType.LeftHand:
                    _animator.SetTrigger(UsableSlot.EquipLeftHandHash);
                    break;
                
                case UsableSlotType.RightHand:
                    _animator.SetTrigger(UsableSlot.EquipRightHandHash);
                    break;
                
                case UsableSlotType.TwoHand:
                    _animator.SetTrigger(UsableSlot.EquipTwoHandHash);
                    break;
            }
        }

        public override void UnEquip()
        {
            base.UnEquip();
            
            StopAllUse();
            
            SetUsable(false);
            
            switch (Reference.SlotType)
            {
                case UsableSlotType.LeftHand:
                    _animator.SetTrigger(UsableSlot.UnEquipLeftHandHash);
                    break;
                
                case UsableSlotType.RightHand:
                    _animator.SetTrigger(UsableSlot.UnEquipRightHandHash);
                    break;
                
                case UsableSlotType.TwoHand:
                    _animator.SetTrigger(UsableSlot.UnEquipTwoHandHash);
                    break;
            }
        }

        public override void EquippedCallback()
        {
            base.EquippedCallback();
            
            SetUsable();
        }
        
        public void Use(UsageType usageType = UsageType.Primary)
        {
            if (!CanUse[usageType]) return;
            
            _usages[usageType]?.Invoke();

            CanUse[usageType] = false;
        }

        public void Stop(UsageType usageType = UsageType.Primary)
        {
            if (CanUse[usageType]) return;
            
            _stoppages[usageType]?.Invoke();
            
            CanUse[usageType] = true;
        }

        public void AddUsage(Action usage, UsageType usageType = UsageType.Primary)
        {
            _usages[usageType] += usage;
        }
        
        public void AddStoppage(Action stoppage, UsageType usageType = UsageType.Primary)
        {
            _stoppages[usageType] += stoppage;
        }
        
        public void RemoveUsage(Action usage, UsageType usageType = UsageType.Primary)
        {
            _usages[usageType] -= usage;
        }
        
        public void RemoveStoppage(Action stoppage, UsageType usageType = UsageType.Primary)
        {
            _stoppages[usageType] -= stoppage;
        }

        protected void SetUsable(bool canUse = true)
        {
            foreach (UsageType key in CanUse.Keys.ToArray())
            {
                CanUse[key] = canUse;
            }
        }

        protected void StopAllUse()
        {
            foreach (UsageType usageType in Utils.GetEnumValues<UsageType>())
            {
                Stop(usageType);
            }
        }
    }
}
