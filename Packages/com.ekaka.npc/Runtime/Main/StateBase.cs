using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC.Main
{
    public enum StateStatus
    {
        //initializing, once initialized it will enable/disable
        Initializing,

        //state is enabled, then it'll be running
        Enabled,

        //state isn't running
        Disabled,
    }

    public abstract class StateBase : MonoBehaviour
    {
        #region StatusChanged

        public delegate void StatusChanged(StateStatus status);

        public event StatusChanged OnStatusChanged;

        private void InvokeStatusChanged(StateStatus status)
        {
            OnStatusChanged?.Invoke(status);
        }

        #endregion

        [field: Tooltip("If true there can only be one of this type per controller")]
        [field: SerializeField] public bool IsUnique { get; private set; } = false;

        //enables state on initialization
        [field: SerializeField] public bool EnableOnInitialize { get; private set; } = false;
        
        [field: SerializeField] public StateUpdate StateUpdate { get; private set; }

        public NPCController Controller { get; private set; }

        public StateStatus Status { get; private set; } = StateStatus.Initializing;
        
        public virtual void Initialize(NPCController controller)
        {
            Controller = controller;

            if (EnableOnInitialize)
            {
                EnableState();
            }

            else
            {
                DisableState();
            }
        }
        
        public virtual void EnableState()
        {
            ChangeStatus(StateStatus.Enabled);
        }

        public abstract void UpdateState();

        public virtual void DisableState()
        {
            ChangeStatus(StateStatus.Disabled);
        }

        public abstract void TryExitState();
        
        private void ChangeStatus(StateStatus newStatus)
        {
            //same state
            if (Status == newStatus)
            {
                return;
            }

            Status = newStatus;

            //invoke event
            InvokeStatusChanged(Status);
        }
    }
}
