using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character.Main;
using Core.Input;
using Core.Utils;
using Inventory.Main;
using Inventory.Main.Item;
using Inventory.Main.Slot;
using Locomotion.Controllers;
using Sensors.Main;
using Sensors.Utils;
using UnityEngine;

namespace Player.Controllers
{
    public class PlayerInventoryController : InventoryController
    {
        [field: SerializeField] public Beamer Beamer { get; private set; }
        
        private BaseInputActions _input;

        public override void Initialize(Actor actor)
        {
            base.Initialize(actor);
            
            _input = InputManager.Instance.InputActions;
            
            RegisterPrimaryUse();
            RegisterSecondaryUse();
            
            //UnEquip OnSprint and Stop Sprint when Equip
            if (actor.GetController(out MotionController motionController))
            {
                if (motionController.IsReady) RegisterLocomotionEvents(motionController);
                
                else motionController.OnReady += delegate { RegisterLocomotionEvents(motionController); };
            }
        }

        private void RegisterPrimaryUse()
        {
            _input.General.PrimaryUse.started += delegate
            {
                bool started = TryUse(UsableSlotType.RightHand);
                
                if (!started) TryUse(UsableSlotType.TwoHand);
            };
            
            _input.General.PrimaryUse.canceled += delegate
            {
                bool canceled = TryStop(UsableSlotType.RightHand);
                
                if (!canceled) TryStop(UsableSlotType.TwoHand);
            };
        }
        
        private void RegisterSecondaryUse()
        {
            _input.General.SecondaryUse.started += delegate
            {
                bool started = TryUse(UsableSlotType.LeftHand);
                
                if (!started) TryUse(UsableSlotType.RightHand, UsageType.Secondary);
                
                if (!started) TryUse(UsableSlotType.TwoHand, UsageType.Secondary);
            };
            
            _input.General.SecondaryUse.canceled += delegate
            {
                bool canceled = TryStop(UsableSlotType.LeftHand);
                
                if (!canceled) TryStop(UsableSlotType.RightHand, UsageType.Secondary);
                
                if (!canceled) TryStop(UsableSlotType.TwoHand, UsageType.Secondary);
            };
        }
        
        //UnEquip OnSprint and Stop Sprint when Equip
        //change look mode to strafe when equipped and unEquip when look mode changed from strafe
        private void RegisterLocomotionEvents(MotionController motionController)
        {
            //if sprint unEquip
            motionController.OnSpeedRateChange += rate =>
            {
                if (rate == MotionController.SpeedRate.Sprint) UnEquipAllUsables();
            };

            //if equip strafe and stop sprint
            OnEquipInitialized += gear =>
            {
                if (gear is IUsable)
                {
                    motionController.ChangeLookMode(MotionController.LookMode.Strafe);
                                    
                    if (motionController.Rate == MotionController.SpeedRate.Sprint)
                    {
                        //switch to Run from Sprint if Equip is initialized (two way)
                        motionController.ChangeSpeedRate(MotionController.SpeedRate.Run);
                    }
                }
            };

            void ChangeLookModeToFree(IGear gear)
            {
                //if gear is usable and all usable slots are unEquipped (change look mode)
                if (gear is IUsable && Usables.All(u => u.Value.Gear == null))
                {
                    motionController.ChangeLookMode(MotionController.LookMode.Free);
                }
            }
            
            //if unEquip look free (not strafe)
            OnUnEquipInitialized += ChangeLookModeToFree;
            OnUnEquipped += ChangeLookModeToFree;
        }
        
        private void Update()
        {
            TryToPick();
            
#if UNITY_EDITOR
            DebugSwitchUsables(_input.General.Change.ReadValue<float>() * - 1f);
#endif
        }
        
        private void TryToPick()
        {
            TargetHit[] hits = Beamer.FindTargets();
        
            if (hits != null && hits.Length > 0)
            {
                TargetHit hit = hits[0];

                //check if item is out of range
                if (Vector3.Distance(hit.Point, transform.position) > interactRadius) return;
            
                if (hit.Collider.TryGetComponent(out IItemAdapter adapter))
                {
                    adapter.Focus();
                
                    if (_input.General.Interact.triggered)
                    {
                        bool added = Bag.Add(adapter.Item, out string message);
                    
                        adapter.Pick(added, message);
                    }
                }
            }
        }

#if UNITY_EDITOR

        private int _inventoryIndex = - 1;
        
        /// <summary>
        /// switch between all Gears in inventory
        /// scroll to last/first item and keep scrolling to unEquip all
        /// </summary>
        /// <param name="direction"></param>
        private void DebugSwitchUsables(float direction)
        {
            int[] indexes = Bag != null ? Bag.Gears.FindIndexes(g => g is IUsable) : 
                Utils.GetEnumValues<UsableSlotType>().Cast<int>().ToArray();
            
            if (direction > 0)
            {
                _inventoryIndex += 1;

                int length = indexes.Length;
                
                if (_inventoryIndex >= length)
                {
                    _inventoryIndex = length;
                    
                    UnEquipAllUsables();
                    
                    return;
                }
                
                if (Bag != null) Equip(indexes[_inventoryIndex]);
                
                else EquipUsableSlot((UsableSlotType) indexes[_inventoryIndex]);
            }

            else if (direction < 0)
            {
                _inventoryIndex -= 1;

                if (_inventoryIndex < 0)
                {
                    _inventoryIndex = - 1;
                    
                    UnEquipAllUsables();
                    
                    return;
                }
                
                if (Bag != null) Equip(indexes[_inventoryIndex]);
                
                else EquipUsableSlot((UsableSlotType) indexes[_inventoryIndex]);
            }
        }
#endif
    }
}
