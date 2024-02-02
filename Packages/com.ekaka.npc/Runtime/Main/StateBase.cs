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

        public StateStatus Status { get; private set; } = StateStatus.Initializing;

        protected virtual void EnableState()
        {
            ChangeStatus(StateStatus.Enabled);
        }

        public abstract void UpdateState();

        protected virtual void DisableState()
        {
            ChangeStatus(StateStatus.Disabled);
        }

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
