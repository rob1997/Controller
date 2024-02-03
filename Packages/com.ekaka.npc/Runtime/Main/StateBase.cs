using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC.Main
{
    public abstract class StateBase : MonoBehaviour
    {
        #region StatusChanged

        public delegate void StatusChanged(bool isEnabled);

        public event StatusChanged OnStatusChanged;

        #endregion

        [field: Tooltip("If true there can only be one of this type per controller")]
        [field: SerializeField] public bool IsUnique { get; private set; } = false;

        //enables state on initialization
        [field: SerializeField] public bool EnableOnInitialize { get; private set; } = false;
        
        [field: SerializeField] public StateUpdate StateUpdate { get; private set; }

        public NPCController Controller { get; private set; }

        public bool IsEnabled { get; private set; }
        
        public virtual void Initialize(NPCController controller)
        {
            Controller = controller;

            if (EnableOnInitialize)
            {
                EnableState();
            }
        }
        
        public virtual void EnableState()
        {
            ChangeStatus(true);
        }

        public abstract void UpdateState();

        public virtual void DisableState()
        {
            ChangeStatus(false);
        }

        public abstract void TryExitState();
        
        private void ChangeStatus(bool isEnabled)
        {
            //same state
            if (IsEnabled == isEnabled)
            {
                return;
            }

            IsEnabled = isEnabled;

            //invoke event
            OnStatusChanged?.Invoke(IsEnabled);
        }
    }
}
