using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Ui.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ui.Main
{
    public class UiLayer : UiElement<UiRoot>
    {
        [field: UiLayerType]
        [field: SerializeField] public string UiLayerType { get; private set; }
    
        public List<UiRegion> UiRegions { get; private set; } = new List<UiRegion>();

        /// <summary>
        /// are all UiRegions in this UiLayer unloaded
        /// </summary>
        public bool AllUiRegionsUnloaded => UiRegions.All(r => r.ActiveUiMenu == null);

        //uiLayer is active if there's an active uiMenu in one if it's uiRegions
        public override bool IsActive => UiRegions.Any(r => r.ActiveUiMenu != null);

        public override void Initialize(UiRoot rootUiElement)
        {
            base.Initialize(rootUiElement);
            
            Debug.Log($"Initializing {UiLayerType} {nameof(UiLayer)}...");
            
            UiRegions = new List<UiRegion>(GetComponentsInChildren<UiRegion>());

            foreach (UiRegion region in UiRegions)
            {
                region.Initialize(this);
            }
        }

        //close all activeUiMenus
        public override void CancelAction()
        {
            //check if there's any activeUiMenus loaded
            if (AllUiRegionsUnloaded)
            {
                return;
            }
            //unload all uiRegions
            UnloadAllUiRegions();
        }

        public bool GetUiRegion(string uiRegionType, out UiRegion uiRegion)
        {
            uiRegion = UiRegions.FirstOrDefault(r => r.UiRegionType == uiRegionType);

            return uiRegion != null;
        }
    
        public bool HasUiRegion(string uiRegionType)
        {
            return UiRegions.Find(r => r.UiRegionType == uiRegionType) != null;
        }

        /// <summary>
        /// unload all uiMenus in layer
        /// </summary>
        /// <param name="onAllUnloaded"></param>
        public void UnloadAllUiRegions(Action onAllUnloaded = null)
        {
            Debug.Log($"Unloading all {nameof(UiRegions)} in {UiLayerType} {nameof(UiLayer)}...");
            
            if (AllUiRegionsUnloaded)
            {
                Debug.LogWarning($"all {nameof(UiRegions)} in {UiLayerType} {nameof(UiLayer)} already unloaded");
                
                onAllUnloaded?.Invoke();
                
                return;
            }
            
            //iterate through all uiRegions with unloaded ActiveUiMenus
            foreach (UiRegion uiRegion in UiRegions.Where(r => r.ActiveUiMenu != null))
            {
                //try unloading every active uiRegion
                uiRegion.TryQueueActiveUiMenuUnload(delegate
                {
                    if (AllUiRegionsUnloaded)
                    {
                        onAllUnloaded?.Invoke();
                        
                        Debug.Log($"Unloaded all {nameof(UiRegions)} in {UiLayerType} {nameof(UiLayer)}...");
                    }
                });
                
                if (AllUiRegionsUnloaded)
                {
                    return;
                }
            }
        }
    }
}
