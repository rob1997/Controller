using System;
using System.Collections;
using System.Collections.Generic;
using Core.Game;
using Core.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Ui.Main
{
    /// <summary>
    /// base uiElement for all custom ui components
    /// </summary>
    /// <typeparam name="T">the parent uiElement used to initialize this UiElement, for example a uiLayer is a rootUiComponent for a uiRegion</typeparam>
    [RequireComponent(typeof(CanvasRenderer))]
    public abstract class UiElement<T> : MonoBehaviour, IUiElement where T : IUiElement
    {
        public abstract bool IsActive { get; }
        
        [field: SerializeField] public bool IsCancelSensitive { get; private set; } = true;

        [field: ShowIf(nameof(IsCancelSensitive))]
        [field: SerializeField] public bool InheritCancelAction { get; private set; } = true;

        private CanvasRenderer _canvasRenderer;
        
        private bool _isQuitting;

        protected UiRoot UiRoot { get; private set; }
        
        /// <summary>
        /// the parent uiElement used to initialize this UiElement
        /// for example a uiLayer is a rootUiComponent for a uiRegion
        /// </summary>
        public T RootUiElement { get; private set; }
        
        public int Depth => _canvasRenderer.absoluteDepth;

        public bool IsNull => this == null || gameObject == null;

        public virtual void Initialize(T rootUiElement)
        {
            RootUiElement = rootUiElement;

            _canvasRenderer = GetComponent<CanvasRenderer>();

            UiRoot = UiManager.Instance.UiRoot;

            if (!UiRoot.AddUiElement(this))
            {
                Debug.LogError($"failed to add {GetType()} to {nameof(UiRoot)}");
            }
        }

        public abstract void CancelAction();

        public IUiElement GetRootUiElement()
        {
            return RootUiElement;
        }

        private void RemoveUiElement()
        {
            //don't remove on destroy if it's quitting
            //UiRoot will already be destroyed (throws error)
            if (!_isQuitting)
            {
                if (!UiRoot.RemoveUiElement(this))
                {
                    Debug.LogError($"failed removing {GetType()} from {nameof(UiRoot)}");
                }
            }
        }

        private void OnApplicationQuit()
        {
            _isQuitting = true;
        }

        private void OnDestroy()
        {
            RemoveUiElement();
        }
    }
}
