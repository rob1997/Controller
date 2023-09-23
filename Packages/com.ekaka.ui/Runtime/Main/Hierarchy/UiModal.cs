using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using TMPro;
using Ui.Main;
using Ui.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Ui.Main
{
    [Serializable]
    public struct UiModalAction
    {
        [field: SerializeField] public string Label { get; private set; }
        
        //UnityEvent because it's serialized on Editor
        [field: SerializeField] public UnityEvent Action { get; private set; }

        /// <summary>
        /// uiModal actions/options for select
        /// </summary>
        /// <param name="label">label/text of the action/option to select</param>
        /// <param name="action">delegate/action that invokes on select</param>
        public UiModalAction(string label, Action action = null)
        {
            Label = label;
            
            if (action != null)
            {
                Action = new UnityEvent();
                //add action delegate to be invoked with Action UnityEvent
                Action.AddListener(action.Invoke);
            }

            else Action = null;
        }
    }
    
    [Serializable]
    public struct UiModalData
    {
        [field: SerializeField] public string Title { get; private set; }
        
        [field: TextArea(2, 4)]
        [field: SerializeField] public string Body { get; private set; }
        
        [field: SerializeField] public UiModalAction[] UiModalActions { get; private set; }

        [field: SerializeField] public bool ForcePick { get; private set; }
        
        [field: SerializeField] public RectTransform BaseRect { get; private set; }
        
        [field: Tooltip("use default padding in UiPreferences.DefaultUiModalPadding")]
        [field: SerializeField] public bool UseDefaultPadding { get; private set; }
        
        [field: ShowIf(nameof(UseDefaultPadding), false)]
        [field: SerializeField] public RectOffset Padding { get; private set; }
        
        [field: SerializeField] public bool UseDefaultUiModal { get; private set; }
        
        [field: UiMenuType(order = 0)]
        [field: SerializeField] public string UiModalMenuType { get; private set; }
        
        [field: SerializeField] public UnityEvent Unloaded { get; private set; }

        /// <summary>
        /// contains values to be attached and displayed on a uiModal
        /// </summary>
        /// <param name="title">title text for uiModal</param>
        /// <param name="body">body/description text for uiModal</param>
        /// <param name="forcePick">force a pick to close/unless you pick an option uiModal can't close</param>
        /// <param name="uiModalActions">uiModal actions for buttons with their label and actions/onClickEvents</param>
        /// <param name="baseRect">base Rect to base of inner panel (stretch inner panel into BaseRect)</param>
        /// <param name="useDefaultPadding">use default padding in UiPreferences.DefaultUiModalPadding</param>
        /// <param name="padding">padding for modal inner Panel</param>
        /// <param name="useDefaultUiModal">use default uiModalMenuType in UiReferences.DefaultUiModalMenuType</param>
        /// <param name="uiModalMenuType">uiMenuType for modal if useDefaultUiModal is false</param>
        /// <param name="onUnloaded">delegate/action for when uiModal is unloaded/onExit</param>
        public UiModalData(string title = default, string body = default, UiModalAction[] uiModalActions = null, bool forcePick = false, 
            RectTransform baseRect = null, bool useDefaultPadding = true, RectOffset padding = null, bool useDefaultUiModal = true, string uiModalMenuType = default, Action onUnloaded = null)
        {
            Title = title;
            
            Body = body;

            UiModalActions = uiModalActions;

            //make sure there's an option to pick for forcePick
            ForcePick = UiModalActions != null && UiModalActions.Length > 0 && forcePick;

            BaseRect = baseRect;
            
            UseDefaultPadding = useDefaultPadding;
            
            Padding = UseDefaultPadding ? null : padding;

            UseDefaultUiModal = useDefaultUiModal;
            
            UiModalMenuType = UseDefaultUiModal ? default : uiModalMenuType;
            
            Unloaded = new UnityEvent();
            
            Unloaded.AddListener(delegate { onUnloaded?.Invoke(); });
        }

        //used for edge case handling
        //forcePick is true when actions is null or empty (nothing to pick)
        public bool GetForcePick()
        {
            return ForcePick && UiModalActions != null && UiModalActions.Length > 0;
        }
        
        //used for edge case handling
        //useDefaultPadding is false when uiModal padding is null
        public bool GetUseDefaultPadding()
        {
            return UseDefaultPadding || Padding == null;
        }
        
        //used for edge case handling
        //useDefaultUiModal is false when uiModalMenuType padding is null or empty
        public bool GetUseDefaultUiModal()
        {
            return UseDefaultUiModal || string.IsNullOrEmpty(UiModalMenuType);
        }
    }
    
    public class UiModal : UiMenu
    {
        [SerializeField] private TextMeshProUGUI _titleLabel;
        
        [SerializeField] private TextMeshProUGUI _bodyLabel;

        [Space]
        
        [SerializeField] private Button _backdropButton;
        
        [SerializeField] private Transform _buttonContainer;
        
        [Space]
        
        [SerializeField] private AssetReference _uiModalButtonReference;

        [SerializeField] private RectTransform _innerPanelRectT;

        private UiModalData _uiModalData;
        
        public override void Initialize(UiRegion rootUiElement)
        {
            base.Initialize(rootUiElement);

            OnUiMenuStateChanged += state =>
            {
                switch (state)
                {
                    case UiMenuState.Loading:
                        //check if UiModal Load is valid (there's a queued uiModal)
                        if (UiRoot.ActiveUiModal == null)
                        {
                            Debug.LogError($"can't load {nameof(UiModalData)}, {nameof(UiRoot.ActiveUiModal)} is null");
                            
                            UiRegion.TryQueueActiveUiMenuUnload();
                            
                            return;
                        }
                        
                        //attach uiModalData
                        InitializeUiModalData(UiRoot.ActiveUiModal.Value);
                        
                        break;
                    
                    case UiMenuState.Unloaded:
                        //invoke onUnloaded
                        //in try catch in case onUnloaded action/delegate throws exception to make sure uiModal exits/unloads safely
                        try
                        {
                            _uiModalData.Unloaded?.Invoke();
                        }
                        
                        catch (Exception e)
                        {
                            Core.Utils.Utils.LogException(e);
                        }
                        
                        //check and load next/queued modal
                        UiRoot.TryLoadNextUiModal();
                        break;
                }
            };
        }

        //attach _uiModalData to text fields and buttons...
        //attach appearance and function to ui
        private void InitializeUiModalData(UiModalData uiModalData)
        {
            _uiModalData = uiModalData;
            
            _titleLabel.text = _uiModalData.Title;
            
            _bodyLabel.text = _uiModalData.Body;

            int actionsCount = _uiModalData.UiModalActions?.Length ?? 0;
            
            //reset in case it's loading from cache
            _backdropButton.onClick.RemoveAllListeners();
            
            //use Get to avoid edge case
            if (!_uiModalData.GetForcePick())
            {
                //add backdrop onClick unload
                _backdropButton.onClick.AddListener(UnloadUiModal);
            }

            //attach buttons with actions
            RecycleAndApplyButtons(actionsCount);
            
            //apply padding
            ApplyRectAndPadding();
        }

        //attach UiModalAction, action and label to a button
        void InitializeButton(Button button, UiModalAction action)
        {
            button.onClick.AddListener(delegate
            {
                //in try catch to avoid ui overlay lock
                //to always unload in case of error
                try
                {
                    action.Action?.Invoke();
                }
                            
                catch (Exception e)
                {
                    Core.Utils.Utils.LogException(e);
                }
                            
                //unload onClick after action
                UnloadUiModal();
            });
            //attach label
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
                
            if (label != null)
            {
                label.text = action.Label;
            }

            else
            {
                Debug.LogWarning($"{nameof(TMP_Text)} component not found on {nameof(UiModal)} button asset");
            }
        }
        
        //invoked when non-recycled/new button is loaded successfully from prefab address
        void ButtonAssetLoaded(Button button)
        {
            int existingButtons = _buttonContainer.childCount;
            
            //un-instantiated buttons length
            int delta = _uiModalData.UiModalActions.Length - existingButtons;
            
            for (int i = 0; i < delta; i++)
            {
                button = Instantiate(button, _buttonContainer);
                //add existing/recycled buttons count to get the right index for instantiated/non-recycled button's action
                int actionIndex = existingButtons + i;
                
                InitializeButton(button, _uiModalData.UiModalActions[actionIndex]);
            }
        }

        //reuse any already existing buttons
        //or instantiate new ones if there's none
        //disable extra/unused ones
        private void RecycleAndApplyButtons(int actionsCount)
        {
            Button button = null;

            void InitializeButton(int actionIndex)
            {
                //activate in case it was disabled
                button.gameObject.SetActive(true);
                //reset onClick before attaching
                button.onClick.RemoveAllListeners();
                            
                this.InitializeButton(button, _uiModalData.UiModalActions[actionIndex]);
            }
            
            //clear/recycle first because uiModal could be cached
            for (int i = 0; i < Math.Max(_buttonContainer.childCount, actionsCount); i++)
            {
                //a button childObject already exists
                if (i < _buttonContainer.childCount)
                {
                    //check for button component
                    if (!_buttonContainer.GetChild(i).TryGetComponent(out button))
                    {
                        Debug.LogError($"{nameof(Button)} component not found on {_buttonContainer.name}[{i}]");
                        
                        continue;
                    }
                    
                    //if there's actions to attach recycle button
                    if (i < actionsCount && _uiModalData.UiModalActions != null)
                    {
                        InitializeButton(i);
                    }
                    //if no more actions to attach disable button
                    else
                    {
                        button.gameObject.SetActive(false);
                    }
                }

                else
                {
                    //if there's actions to attach, instantiate new button
                    if (i < actionsCount && _uiModalData.UiModalActions != null)
                    {
                        //there's a button already instantiated, recycle that
                        if (button != null)
                        {
                            //instantiate/copy new button
                            button = Instantiate(button, _buttonContainer);
                            
                            InitializeButton(i);
                        }
                        
                        else
                        {
                            //load button asset and instantiate for each unattached action
                            Core.Utils.Utils.LoadObjComponent<Button>(_uiModalButtonReference.AssetGUID, ButtonAssetLoaded);
                            //break out because the above method is async
                            break;
                        }
                    }

                    else
                    {
                        //this shouldn't happen unless above code was altered (written just for error handling purposes)
                        Debug.LogError($"index {i} can't be greater than both {actionsCount} {nameof(UiModalAction)}s and {_buttonContainer.childCount} {nameof(Button)}s count");
                    }
                }
            }
        }
        
        //apply base rect and padding to inner panel's rect
        //change anchored position and size
        private void ApplyRectAndPadding()
        {
            //base rect is what padding will be based off of
            RectTransform baseRectTransform = _uiModalData.BaseRect != null ? _uiModalData.BaseRect : (RectTransform) transform;

            Rect baseRect = baseRectTransform.rect;
            
            RectOffset padding = _uiModalData.Padding;
            
            //try set default padding
            //use Get to avoid edge case
            if (_uiModalData.GetUseDefaultPadding())
            {
                padding = UiManager.UiPreferences.DefaultUiModalPadding;
            }

            Vector2 innerPanelPivot = _innerPanelRectT.pivot;
            
            //apply padding to position
            //factor in pivot
            float paddingDeltaX = (padding.horizontal * innerPanelPivot.x) - padding.right;
            
            float paddingDeltaY = (padding.vertical * innerPanelPivot.y) - padding.top;
            
            //set position, use transform position to avoid different anchoring type issues
            //different types of anchors have different anchoredPositions (x, y) (top, left)...
            //factor in pivot and padding
            _innerPanelRectT.position = (Vector2) baseRectTransform.position + (baseRect.size * (innerPanelPivot - baseRectTransform.pivot)) + new Vector2(paddingDeltaX, paddingDeltaY);
            
            //deduct paddings from width/height
            float width = baseRect.width - padding.horizontal;
            
            float height = baseRect.height - padding.vertical;

            //apply size with current anchors
            _innerPanelRectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            
            _innerPanelRectT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        }

        public override void CancelAction()
        {
            Debug.Log($"performing {nameof(CancelAction)} on {UiMenuType} {nameof(UiModal)}");
            
            //if it's forcePick, must pick
            if (!_uiModalData.ForcePick)
            {
                UnloadUiModal();
            }
        }

        private void UnloadUiModal()
        {
            //check in case click/pick action had an unload
            if (UiMenuState != UiMenuState.Unloaded)
            {
                //unload from region
                UiRegion.TryQueueActiveUiMenuUnload();
            }
        }
    }
}
