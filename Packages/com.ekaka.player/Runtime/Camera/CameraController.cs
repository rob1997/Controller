using System;
using System.Collections;
using System.Collections.Generic;
using Character.Main;
using Cinemachine;
using Inventory.Main;
using Inventory.Main.Item;
using UnityEngine;
using Weapon.Main;

namespace Player.VirtualCamera
{
    public class CameraController : Controller
    {
        [SerializeField] private VirtualCameraWrapper lookWrapper;
    
        [SerializeField] private VirtualCameraWrapper zoomWrapper;
        
        [SerializeField] private float zoomBlend = .25f;
    
        private VirtualCameraWrapper _activeWrapper;
    
        [field: SerializeField] public CinemachineBrain Brain { get; private set; }

        private InventoryController _inventoryController;
        
        public override void Initialize(Actor actor)
        {
            if (Brain.ActiveVirtualCamera != null 
                && Brain.ActiveVirtualCamera is CinemachineVirtualCamera vCam)
            {
                _activeWrapper = vCam.GetComponent<VirtualCameraWrapper>();
            }

            if (actor.GetController(out _inventoryController))
            {
                if (_inventoryController.IsReady)
                    RegisterFirearmUsage();

                else
                    _inventoryController.OnReady += RegisterFirearmUsage;
            }

            else
            {
                Debug.LogWarning($"{nameof(InventoryController)} not found on {actor.gameObject.name}");
            }
        }

        public void ZoomIn()
        {
            Switch(zoomWrapper, zoomBlend);
        }
        
        public void ZoomOut()
        {
            Switch(lookWrapper, zoomBlend);
        }
        
        public void Switch(VirtualCameraWrapper wrapper, float switchTime)
        {
            Brain.m_DefaultBlend.m_Time = switchTime;
        
            if (_activeWrapper != null)
            {
                //already on same cam (don't need to switch)
                if (_activeWrapper == wrapper) return;
                
                _activeWrapper.VirtualCamera.Priority = - 1;
            }
        
            wrapper.VirtualCamera.Priority = 1;

            _activeWrapper = wrapper;
        }

        public void Shake(Vector3 tick, float tickDuration, float cooldownDuration)
        {
            if (_activeWrapper != null) _activeWrapper
                .Shake(tick, tickDuration, cooldownDuration);
        }
    
        public void Gain(Vector3 gain, float duration)
        {
            if (_activeWrapper != null) _activeWrapper
                .LookGain(gain, duration);
        }

        //registers the zoom in and out effect (ADS - aim down sight) for player held firearms
        //register camera shake and recoil gain on firearm shoot
        private void RegisterFirearmUsage()
        {
            void TryRegisterUsage(IGear gear, bool add)
            {
                if (gear is Firearm firearm)
                {
                    FirearmAdapter firearmAdapter = (FirearmAdapter) _inventoryController.Usables[firearm.SlotType].Adapter;

                    FirearmReference firearmReference = (FirearmReference) firearm.Reference;

                    void ShakeAndGain()
                    {
                        Gain(firearmAdapter.GetRecoilGain(), firearmReference.TotalDuration);

                        Shake(firearmAdapter.GetShakeValue(), firearmReference.FireDuration, firearmReference.CooldownDuration);
                    }

                    if (add)
                    {
                        firearmAdapter.AddUsage(ShakeAndGain);
                        
                        firearmAdapter.AddUsage(ZoomIn, UsageType.Secondary);
                        
                        firearmAdapter.AddStoppage(ZoomOut, UsageType.Secondary);
                    }

                    else
                    {
                        firearmAdapter.RemoveUsage(ShakeAndGain);
                        
                        firearmAdapter.RemoveUsage(ZoomIn, UsageType.Secondary);
                        
                        firearmAdapter.RemoveStoppage(ZoomOut, UsageType.Secondary);
                    }
                }
            }
            
            _inventoryController.OnEquipped += gear =>
            {
                TryRegisterUsage(gear, true);
            };
                
            _inventoryController.OnUnEquipped += gear =>
            {
                TryRegisterUsage(gear, false);
            };
        }
    }
}
