using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Game;
using Ui.Main;
using Ui.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Ui.Main
{
    public enum UnloadMode
    {
        //destroy
        Remove,

        //disable
        Cache,
    }

    public enum UiMenuState
    {
        Loading,
        Loaded,
        Unloading,
        Unloaded
    }

    public class UiMenu : UiElement<UiRegion>
    {
        #region UiMenuStateChanged

        public delegate void UiMenuStateChanged(UiMenuState newState);

        public event UiMenuStateChanged OnUiMenuStateChanged;

        private void InvokeUiMenuStateChanged(UiMenuState newState)
        {
            OnUiMenuStateChanged?.Invoke(newState);
        }

        #endregion

        [field: UiLayerType]
        [field: SerializeField] public string UiLayerType { get; private set; }
        
        [field: UiRegionType]
        [field: SerializeField] public string UiRegionType { get; private set; }
        
        [field: UiMenuType]
        [field: SerializeField] public string UiMenuType { get; private set; }
        
        [field: SerializeField] public UnloadMode UnloadMode { get; private set; }
        
        [field: SerializeField] public UiTransition UiTransition { get; private set; }

        //stat/instantiate menu unloaded
        public UiMenuState UiMenuState { get; private set; } = UiMenuState.Unloaded;

        /// <summary>
        /// coroutine that destroys cached menu after <see cref="UiPreferences.MenuCacheTimeOut"/> of being cached
        /// </summary>
        private Coroutine _destroyCacheCoroutine;

        /// <summary>
        /// call each delegate when <see cref="UiMenuState"/> changes
        /// </summary>
        private readonly Dictionary<UiMenuState, Action> _queuedUiMenuActions = new Dictionary<UiMenuState, Action>();

        public UiRegion UiRegion => RootUiElement;

        public UiLayer UiLayer => UiRegion.UiLayer;

        protected UiManager UiManager { get; private set; }
        
        //uiMenu is active if it's loaded
        public override bool IsActive => UiMenuState == UiMenuState.Loaded;

        public override void Initialize(UiRegion rootUiElement)
        {
            base.Initialize(rootUiElement);
            
            Debug.Log($"Initializing {UiRegion.UiLayer.UiLayerType}-{UiRegion.UiRegionType}-{UiMenuType} {nameof(UiMenu)}...");
            
            UiManager = UiManager.Instance;

            if (UiTransition == null)
            {
                Debug.LogWarning($"{nameof(UiTransition)} not found on {UiMenuType} {nameof(UiMenu)}, assigning default {UiManager.UiPreferences.DefaultUiTransition}");
                    
                UiTransition = UiManager.UiPreferences.DefaultUiTransition;
            }

            UiTransition.Setup(this);

            OnUiMenuStateChanged += state => { UiRegion.InvokeUiMenuStateChanged(this); };
        }

        public override void CancelAction()
        {
            Debug.Log($"performing {nameof(CancelAction)} on {UiMenuType} {nameof(UiMenu)}");
            
            UiRegion.TryQueueActiveUiMenuUnload();
        }

        /// <summary>
        /// load uiMenu
        /// </summary>
        /// <param name="onLoaded">action called after uiMenu is loaded</param>
        public void LoadUiMenu(Action onLoaded = null)
        {
            switch (UiMenuState)
            {
                case UiMenuState.Unloading:

                    //queue load action on unloaded
                    QueueUiMenuAction(UiMenuState.Unloaded, delegate { LoadUiMenu(onLoaded); });
                    //once queued return/gets called on queue
                    return;
                
                case UiMenuState.Loading: case UiMenuState.Loaded:
                    
                    Debug.LogWarning($"can't load {UiMenuType} {nameof(UiMenu)}, already {UiMenuState}");
                    return;
            }
            //queue onLoaded action
            QueueUiMenuAction(UiMenuState.Loaded, onLoaded);
            
            gameObject.SetActive(true);
            //bring to front
            gameObject.transform.SetAsLastSibling();

            if (_destroyCacheCoroutine != null) UiRegion.StopCoroutine(_destroyCacheCoroutine);

            ChangeUiMenuState(UiMenuState.Loading);
            
            if (UiTransition != null)
            {
                UiTransition.Load(this, delegate
                {
                    ChangeUiMenuState(UiMenuState.Loaded);
                });
            }

            else
            {
                Debug.LogWarning($"{nameof(UiTransition)} for {UiMenuType} {nameof(UiMenu)} not found");
                
                ChangeUiMenuState(UiMenuState.Loaded);
            }
        }

        public void UnloadUiMenu(Action onUnloaded = null)
        {
            switch (UiMenuState)
            {
                case UiMenuState.Loading:
                    
                    //queue unload action on loaded
                    QueueUiMenuAction(UiMenuState.Loaded, delegate { UnloadUiMenu(onUnloaded); });
                    //once queued return/gets called on queue
                    return;
                
                case UiMenuState.Unloading: case UiMenuState.Unloaded:
                    
                    Debug.LogWarning($"can't unload {UiMenuType} {nameof(UiMenu)}, already {UiMenuState}");
                    return;
            }
            //queue onUnloaded action
            QueueUiMenuAction(UiMenuState.Unloaded, onUnloaded);
            
            ChangeUiMenuState(UiMenuState.Unloading);
            
            if (UiTransition != null)
            {
                UiTransition.Unload(this, delegate
                {
                    ChangeUiMenuState(UiMenuState.Unloaded);
                    //call after state change to cache or remove
                    Unloaded();
                });
            }

            else
            {
                Debug.LogWarning($"{nameof(UiTransition)} for {UiMenuType} {nameof(UiMenu)} not found");
                
                ChangeUiMenuState(UiMenuState.Unloaded);
                //call after state change to cache or remove
                Unloaded();
            }
        }

        private void ChangeUiMenuState(UiMenuState newState)
        {
            if (UiMenuState == newState)
            {
                return;
            }

            Debug.Log($"{newState} {UiMenuType} {nameof(UiMenu)} from {UiMenuState}");
            
            UiMenuState = newState;
            
            //call on queued actions
            InvokeQueuedUiMenuAction();
            
            InvokeUiMenuStateChanged(UiMenuState);
        }

        /// <summary>
        /// queue action on uiMenu state changed
        /// </summary>
        /// <param name="uiMenuState">new changed UiMenuState</param>
        /// <param name="action">action to be invoked on new state change</param>
        public void QueueUiMenuAction(UiMenuState uiMenuState, Action action)
        {
            if (!_queuedUiMenuActions.ContainsKey(uiMenuState))
            {
                _queuedUiMenuActions.Add(uiMenuState, action);
            }

            else _queuedUiMenuActions[uiMenuState] = action;
        }
        
        private void InvokeQueuedUiMenuAction()
        {
            //make sure action exists before invoking
            if (_queuedUiMenuActions.ContainsKey(UiMenuState) && _queuedUiMenuActions[UiMenuState] != null)
            {
                _queuedUiMenuActions[UiMenuState]?.Invoke();
                //reset action after invoking
                _queuedUiMenuActions[UiMenuState] = null;
            }
        }
        
        private void Unloaded()
        {
            if (UiMenuState != UiMenuState.Unloaded)
            {
                //could happen if trying to reload uiMenu while already loaded
                Debug.LogWarning($"Can't unload {UiMenuType} {nameof(UiMenu)} when {nameof(UiMenuState)} is {UiMenuState}");
                
                return;
            }
            
            switch (UnloadMode)
            {
                case UnloadMode.Remove:
                    Remove();
                    break;
                case UnloadMode.Cache:
                    Cache();
                    break;
            }
        }

        private void Remove()
        {
            Debug.Log($"Removing {UiMenuType} {nameof(UiMenu)}...");
            //remove from region too
            UiRegion.UiMenus.Remove(this);

            Destroy(gameObject);
        }

        //useful for continuity or performance
        private void Cache()
        {
            Debug.Log($"Caching {UiMenuType} {nameof(UiMenu)}...");
            
            gameObject.SetActive(false);

            _destroyCacheCoroutine = UiRegion.StartCoroutine(WaitThenRemove());
        }

        IEnumerator WaitThenRemove()
        {
            yield return new WaitForSeconds(UiManager.UiPreferences.MenuCacheTimeOut);

            Remove();
        }
    }
}