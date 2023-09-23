using System;
using System.Collections.Generic;
using System.Linq;
using Core.Game;
using Core.Input;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ui.Main
{
    public class UiRoot : MonoBehaviour, IUiElement
    {
        [SerializeField] private UiModalData _exitUiModal;
        
        [field: SerializeField] public RectTransform LayerRoot { get; private set; }
        
        [field: SerializeField] public Canvas Canvas { get; private set; }
        
        public List<UiLayer> UiLayers { get; private set; }

        public bool AllUiLayersUnloaded => UiLayers.All(l => l.AllUiRegionsUnloaded);
        
        //depth is always - 1 since root in front of everything else
        public int Depth => - 1;
        
        //uiRoot is always cancel insensitive (on Cancel/esc/back Input performed)
        public bool IsCancelSensitive => true;
        
        //uiRoot can't inherit cancel action since it's the bottom most/root UiElement of all UiElements
        public bool InheritCancelAction => false;
        
        public bool IsNull => this == null || gameObject == null;
        
        //uiRoot is always active
        public bool IsActive => true;
        
        private UiManager _uiManager;

        private InputManager _inputManager;
        
        private IUiElement[] _allUiElements = { };

        //list of all queued active uiModals
        private readonly Queue<UiModalData> _queuedUiModals = new Queue<UiModalData>();
        
        //active/loaded uiModal, nullable to make check if able to load next queued uiModal
        public UiModalData? ActiveUiModal { get; private set; } = null;
        
        public void Initialize(UiManager uiManager)
        {
            Debug.Log($"Initializing {nameof(UiRoot)}...");
            
            _uiManager = uiManager;

            if (LayerRoot == null) LayerRoot = GetComponentInChildren<RectTransform>();

            if (Canvas == null) Canvas = GetComponentInParent<Canvas>();
            
            //ui inputs like back/cancel/select...
            InitializeInput();
            
            UiLayers = new List<UiLayer>(LayerRoot.GetComponentsInChildren<UiLayer>());

            foreach (UiLayer layer in UiLayers)
            {
                layer.Initialize(this);
            }

            //add uiRoot to _allUiElements array
            _allUiElements = _allUiElements.Append(this).ToArray();
        }

        //assign input actions
        private void InitializeInput()
        {
            if (GameManager.Instance.IsReady)
            {
                RegisterUiInputActions();
            }

            else
            {
                GameManager.Instance.OnReady += RegisterUiInputActions;
            }
            
            void RegisterUiInputActions()
            {
                _inputManager = InputManager.Instance;
                
                if (_inputManager.IsReady)
                {
                    AddUiInputActions();
                }

                else
                {
                    _inputManager.OnReady += AddUiInputActions;
                }
            }
            
            void AddUiInputActions()
            {
                //subscribe to input actions changed
                //because inputActions is disposed, initialized and re-initialized base on GameManager.GameState changes we can't just call this once
                _inputManager.OnInputActionsInitialized += actions =>
                {
                    actions.UI.Cancel.performed += delegate { Cancel(); };
                
                    Debug.Log($"{nameof(actions.UI.Cancel)} Ui action added");
                };
            }
        }

        /// <summary>
        /// load ui menu from prefab/addressable
        /// </summary>
        /// <param name="uiMenuType">a UiMenuType in UiReferences.UiMenuReferences</param>
        /// <param name="uiRegionType">a UiRegionType in UiReferences.UiRegionTypes</param>
        /// <param name="uiLayerType">a UiLayerType in UiReferences.UiLayerTypes</param>
        /// <param name="onUiMenuLoaded">action invoked when uiMenu is loaded</param>
        public void LoadUiMenu(string uiMenuType, string uiRegionType = null, string uiLayerType = null, Action onUiMenuLoaded = null)
        {
            LoadUiMenu<UiMenu>(uiMenuType, uiRegionType, uiLayerType, onUiMenuLoaded);
        }
        
        //used to load specific type uiMenu like uiModal
        private void LoadUiMenu<T>(string uiMenuType, string uiRegionType = null, string uiLayerType = null, Action onUiMenuLoaded = null) where T : UiMenu
        {
            if (_uiManager.GetMenuReference(uiMenuType, out AssetReference menuRef))
            {
                Core.Utils.Utils.LoadObjComponent<T>(menuRef.AssetGUID, uiMenu =>
                {
                    //if parameter type is null assign to prefab layer type
                    uiLayerType = string.IsNullOrEmpty(uiLayerType) ? uiMenu.UiLayerType : uiLayerType;
                        
                    uiRegionType = string.IsNullOrEmpty(uiRegionType) ? uiMenu.UiRegionType : uiRegionType;
                        
                    //load menu
                    if (GetUiRegion(uiLayerType, uiRegionType, out UiRegion uiRegion))
                    {
                        //load from cached uiMenu if possible
                        if (uiRegion.HasUiMenu(uiMenuType)) uiRegion.LoadUiMenu(uiMenuType, onUiMenuLoaded);
                        //load from prefab
                        else uiRegion.LoadUiMenu(uiMenu, onUiMenuLoaded);
                    }

                    else Debug.LogError($"{uiRegionType} {nameof(UiRegion)} not found in {uiLayerType} {nameof(UiLayer)}");
                });
            }

            else Debug.LogError($"Can't find prefab address for {uiMenuType} {nameof(UiMenu)}");
        }

        /// <summary>
        /// unload all UiMenus in uiRoot
        /// </summary>
        /// <param name="onAllUnloaded"></param>
        public void UnloadAllLayers(Action onAllUnloaded = null)
        {
            Debug.Log($"Unloading all {nameof(UiLayers)}...");
            
            if (AllUiLayersUnloaded)
            {
                Debug.LogWarning($"all {nameof(UiLayers)} already unloaded");
                
                onAllUnloaded?.Invoke();
                
                return;
            }
            
            //iterate through all uiLayers with unloaded uiRegions
            foreach (UiLayer uiLayer in UiLayers.Where(l => !l.AllUiRegionsUnloaded))
            {
                //try unloading all uiRegions
                uiLayer.UnloadAllUiRegions(delegate
                {
                    if (AllUiLayersUnloaded)
                    {
                        onAllUnloaded?.Invoke();
                        
                        Debug.Log($"Unloaded all {nameof(UiLayers)}");
                    }
                });

                if (AllUiLayersUnloaded)
                {
                    return;
                }
            }
        }

        public void QueueUiModal(UiModalData uiModalData)
        {
            _queuedUiModals.Enqueue(uiModalData);

            //no loaded uiModal
            if (ActiveUiModal == null)
            {
                TryLoadNextUiModal();
            }
        }

        public void TryLoadNextUiModal()
        {
            //unload active uiModal
            if (ActiveUiModal != null) ActiveUiModal = null;
            
            if (_queuedUiModals.Count <= 0)
            {
                return;
            }

            ActiveUiModal = _queuedUiModals.Dequeue();

            UiModalData uiModalData = ActiveUiModal.Value;
            
            string uiModalMenuType = _uiManager.UiReferences.DefaultUiModalMenuType;
            
            //check if UiModal is using custom UiModalMenuType
            //use Get to avoid edge case
            if (!uiModalData.GetUseDefaultUiModal())
            {
                uiModalMenuType = uiModalData.UiModalMenuType;
            }
            
            LoadUiMenu<UiModal>(uiModalMenuType);
        }
        
        public bool HasUiLayer(string uiLayerType)
        {
            return UiLayers.Find(l => l.UiLayerType == uiLayerType) != null;
        }

        public bool GetUiLayer(string uiLayerType, out UiLayer uiLayer)
        {
            uiLayer = UiLayers.FirstOrDefault(l => l.UiLayerType == uiLayerType);

            return uiLayer != null;
        }

        public bool GetUiRegion(string uiLayerType, string uiRegionType, out UiRegion uiRegion)
        {
            uiRegion = null;

            if (GetUiLayer(uiLayerType, out UiLayer uiLayer) && uiLayer.GetUiRegion(uiRegionType, out uiRegion))
            {
                return uiRegion != null;
            }

            else return false;
        }

        public bool AddUiElement(IUiElement uiElement)
        {
            if (_allUiElements.Contains(uiElement))
            {
                return false;
            }
            
            _allUiElements = _allUiElements.Append(uiElement).ToArray();

            return true;
        }
        
        public bool RemoveUiElement(IUiElement uiElement)
        {
            //remove if it's contained
            if (_allUiElements.Contains(uiElement))
            {
                _allUiElements = _allUiElements.Where(u => u != uiElement).ToArray();
                
                return true;
            }
            
            return false;
        }

        //get front most uiElement based on z-indexing/furthest child in hierarchy
        public IUiElement GetFrontMostUiElement()
        {
            return _allUiElements.OrderByDescending(e => e.Depth).FirstOrDefault();
        }
        
        public void CancelAction()
        {
            //exit game
            Debug.Log($"Performing root {nameof(BaseInputActions.UI.Cancel)} action...");
            //load exit uiModal
            QueueUiModal(_exitUiModal);
        }

        //since UiRoot is the root most UiElement it returns itself
        public IUiElement GetRootUiElement()
        {
            return this;
        }
        
        //get and invoke cancel action on a front most, active and cancel sensitive UiElement
        private void Cancel()
        {
            //get top most active and cancel sensitive uiElement
            IUiElement uiElement = _allUiElements.Where(e => e.IsActive && e.IsCancelSensitive).OrderByDescending(e => e.Depth).FirstOrDefault();
             
            if (uiElement != null && !uiElement.IsNull)
            {
                //if uiElement is cancel sensitive but inherits cancel action from rootUiElement or if it's not cancel sensitive or not active keep looking up the hierarchy
                //this can go all the way up to uiRoot where exit modal is loaded from UiReferences.ExitUiModalDict
                while (!uiElement.IsCancelSensitive || !uiElement.IsActive || (uiElement.IsCancelSensitive && uiElement.InheritCancelAction))
                {
                    uiElement = uiElement.GetRootUiElement();

                    if (uiElement == null || uiElement.IsNull)
                    {
                        Debug.LogError($"{nameof(GetRootUiElement)} failed");
                                
                        return;
                    }
                }
                 
                Debug.Log($"Performing {nameof(BaseInputActions.UI.Cancel)} action on {uiElement}...");
                
                uiElement.CancelAction();
            }

            else
            {
                Debug.LogError($"can't find top most {nameof(IUiElement)}");
            }
        }
    }
}