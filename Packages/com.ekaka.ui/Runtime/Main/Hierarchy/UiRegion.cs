using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Game;
using Core.Utils;
using Ui.Main;
using Ui.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Serialization;

namespace Ui.Main
{
    public class UiRegion : UiElement<UiLayer>
    {
        #region UiMenuStateChanged

        public delegate void UiMenuStateChanged(UiMenu uiMenu);

        public event UiMenuStateChanged OnUiMenuStateChanged;

        public void InvokeUiMenuStateChanged(UiMenu uiMenu)
        {
            OnUiMenuStateChanged?.Invoke(uiMenu);
        }

        #endregion

        [field: UiRegionType]
        [field: SerializeField] public string UiRegionType { get; private set; }

        public List<UiMenu> UiMenus { get; private set; } = new List<UiMenu>();

        public UiLayer UiLayer => RootUiElement;

        public UiMenu ActiveUiMenu { get; private set; }

        //uiRegion is active if a uiMenu is already active in the uiRegion
        //and if load stack has a count more than 1 (isn't the last/first uiMenu loaded - nothing to go back to)
        public override bool IsActive => ActiveUiMenu != null && _loadStack.Count > 1;
        
        private UiManager _uiManager;
        
        //stack of all loaded uiMenus is first in last out order
        //used for back/cancel/esc action
        private readonly Stack<string> _loadStack = new Stack<string>();
        
        public override void Initialize(UiLayer rootUiElement)
        {
            base.Initialize(rootUiElement);
            
            Debug.Log($"Initializing {UiLayer.UiLayerType}-{UiRegionType} {nameof(UiRegion)}...");

            _uiManager = UiManager.Instance;

            OnUiMenuStateChanged += uiMenu =>
            {
                switch (uiMenu.UiMenuState)
                {
                    //add uiMenuType to _loadStack on loaded
                    case UiMenuState.Loaded:
                        
                        string uiMenuType = uiMenu.UiMenuType;
                        //can't push the same uiMenu consecutively unless it's already empty
                        if (_loadStack.Count <= 0 || _loadStack.Peek() != uiMenuType) _loadStack.Push(uiMenuType);
                        
                        break;
                }
            };
        }

        //use stacks to load last loaded uiMenu
        public override void CancelAction()
        {
            //check if stack has uiMenus
            //can't unload last uiMenu
            if (_loadStack.Count <= 1)
            {
                return;
            }

            TryQueueActiveUiMenuUnload(delegate
            {
                //when unloaded pop current uiMenu
                _loadStack.Pop();
                //load previous uiMenu
                LoadUiMenu(_loadStack.Peek());
            });
        }

        public bool HasUiMenu(string uiMenuType)
        {
            return UiMenus.FirstOrDefault(m => m.UiMenuType == uiMenuType) != null;
        }

        public bool GetUiMenu(string uiMenuType, out UiMenu uiMenu)
        {
            uiMenu = UiMenus.FirstOrDefault(m => m.UiMenuType == uiMenuType);

            return uiMenu != null;
        }

        /// <summary>
        /// queue uiMenu load action
        /// </summary>
        /// <param name="uiMenu"></param>
        /// <param name="onLoaded"></param>
        private void TryQueueUiMenuLoad(UiMenu uiMenu, Action onLoaded = null)
        {
            Debug.Log($"Queueing load for {uiMenu.UiMenuType} {nameof(UiMenu)}...");
            
            //load now
            void LoadImmediate()
            {
                ActiveUiMenu = uiMenu;
                                
                uiMenu.LoadUiMenu(onLoaded);
            }
            
            if (ActiveUiMenu != null)
            {
                //unload first then load on unloaded
                void UnloadThenLoad()
                {
                    TryQueueActiveUiMenuUnload(LoadImmediate);
                }

                UiMenuState activeUiMenuState = ActiveUiMenu.UiMenuState;
                
                switch (activeUiMenuState)
                {
                    case UiMenuState.Loading:
                        //on active menu loaded unload then load on unloaded
                        ActiveUiMenu.QueueUiMenuAction(UiMenuState.Loaded, UnloadThenLoad);
                        break;
                    
                    case UiMenuState.Loaded:
                        //unload active menu then load on unloaded
                        UnloadThenLoad();
                        break;
                    
                    case UiMenuState.Unloading:
                        //on active menu unloaded load
                        ActiveUiMenu.QueueUiMenuAction(UiMenuState.Unloaded, LoadImmediate);
                        break;
                    
                    case UiMenuState.Unloaded:
                        //ActiveUiMenu should be null
                        Debug.LogWarning($"{activeUiMenuState} {nameof(ActiveUiMenu)} {ActiveUiMenu.UiMenuType}");
                        //already unloaded so load
                        LoadImmediate();
                        break;
                }
            }

            else
            {
                LoadImmediate();
            }
        }
        
        /// <summary>
        /// queue unload ActiveUiMenu
        /// </summary>
        /// <param name="onUnloaded"></param>
        public void TryQueueActiveUiMenuUnload(Action onUnloaded = null)
        {
            if (ActiveUiMenu != null)
            {
                Debug.Log($"Queueing unload for {ActiveUiMenu.UiMenuType} {nameof(UiMenu)}...");
                
                //call on ActiveUiMenu unloaded
                void Unloaded()
                {
                    ActiveUiMenu = null;
                        
                    onUnloaded?.Invoke();
                }
                //unload then call Unloaded on unloaded
                void UnloadImmediate()
                {
                    ActiveUiMenu.UnloadUiMenu(Unloaded);
                }

                UiMenuState activeUiMenuState = ActiveUiMenu.UiMenuState;
                
                switch (activeUiMenuState)
                {
                    case UiMenuState.Loading:
                        //unload on loaded
                        ActiveUiMenu.QueueUiMenuAction(UiMenuState.Loaded, UnloadImmediate);
                        break;
                    
                    case UiMenuState.Loaded:
                        //unload now
                        UnloadImmediate();
                        break;
                    
                    case UiMenuState.Unloading:
                        //on unloaded invoke unloaded
                        ActiveUiMenu.QueueUiMenuAction(UiMenuState.Unloaded, Unloaded);
                        break;
                    
                    case UiMenuState.Unloaded:
                        //ActiveUiMenu should be null
                        Debug.LogWarning($"{activeUiMenuState} {nameof(ActiveUiMenu)} {ActiveUiMenu.UiMenuType}");
                        Unloaded();
                        break;
                }
            }

            else
            {
                //already unloaded/null
                Debug.LogWarning($"{nameof(ActiveUiMenu)} already unloaded/null");
            }
        }

        public void LoadUiMenu(string uiMenuType, Action onLoaded = null)
        {
            if (GetUiMenu(uiMenuType, out UiMenu uiMenu))
            {
                TryQueueUiMenuLoad(uiMenu, onLoaded);
            }

            else
            {
                //instantiate and add uiMenu before load
                _uiManager.UiRoot.LoadUiMenu(uiMenuType, UiRegionType, UiLayer.UiLayerType, onLoaded);
            }
        }

        public void LoadUiMenu(UiMenu uiMenu, Action onLoaded = null)
        {
            string uiMenuType = uiMenu.UiMenuType;

            if (HasUiMenu(uiMenuType))
            {
                GetUiMenu(uiMenuType, out uiMenu);
            }
            
            //uiMenu from prefab
            else
            {
                Debug.Log($"Adding {uiMenu.UiMenuType} {nameof(UiMenu)} to {UiRegionType} {nameof(UiRegion)}...");
                
                uiMenu = Instantiate(uiMenu, transform);

                UiMenus.Add(uiMenu);

                uiMenu.Initialize(this);
            }

            TryQueueUiMenuLoad(uiMenu, onLoaded);
        }
    }
}