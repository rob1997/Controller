using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Inventory.Main;
using Inventory.Main.Item;
using Inventory.Main.Slot;
using Ui.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Ui
{
    //uiMenu in the middle region of the hud,
    //contains crosshair, item wheel...
    public class HudMainUiMenu : UiMenu
    {
        [SerializeField] private Image _crosshairImage;

        private Camera _mainCamera;
        
        public override void Initialize(UiRegion rootUiElement)
        {
            base.Initialize(rootUiElement);
            
            _mainCamera = Camera.main;
            
            Character.Player player = Character.Player.Instance;
            
            if (player.IsReady)
            {
                InitializeCrosshair(player);
            }

            else
            {
                player.OnReady += delegate
                {
                    InitializeCrosshair(player);
                };
            }
        }

        private void InitializeCrosshair(Character.Player player)
        {
            if (!player.GetController(out InventoryController inventoryController))
            {
                Debug.LogError($"{nameof(InventoryController)} not found in {nameof(Character.Player)}");
                
                return;
            }
            
            void SlotUpdated(bool equipped)
            {
                if (equipped)
                {
                    //setup position based on beamer position
                    _crosshairImage.transform.position = _mainCamera.WorldToScreenPoint(player.Targeter.Muzzle.position);
                    
                    _crosshairImage.gameObject.SetActive(true);
                }

                else
                {
                    //if all usable slots are unequipped disable crosshair
                    if (inventoryController.Usables.All(u => u.Value.State == SlotState.UnEquipped))
                    {
                        _crosshairImage.gameObject.SetActive(false);
                    }
                }
            }
            //update crosshair on inventory updates
            void RegisterSlotUpdated()
            {
                inventoryController.OnEquipInitialized += gear =>
                {
                    //make sure it's a usable that's equipping
                    if (gear is IUsable) SlotUpdated(true);
                };
                
                inventoryController.OnUnEquipped += gear =>
                {
                    //make sure it's a usable that's unequipped
                    if (gear is IUsable) SlotUpdated(false);
                };
            }
            
            //register to inventory
            if (inventoryController.IsReady)
            {
                RegisterSlotUpdated();
            }

            else
            {
                inventoryController.OnReady += RegisterSlotUpdated;
            }
            
            //disable crosshair on initialize
            SlotUpdated(false);
        }
    }
}
