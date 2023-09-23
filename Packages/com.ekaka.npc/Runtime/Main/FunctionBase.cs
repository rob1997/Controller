using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC.Main
{
    public enum FunctionState
    {
        //initializing, once initialized it will enable/disable
        Initializing,

        //function is enabled, then it'll be running
        Enabled,

        //function isn't running
        Disabled,
    }

    public abstract class FunctionBase : MonoBehaviour
    {
        #region StateChanged

        public delegate void StateChanged(FunctionState state);

        public event StateChanged OnStateChanged;

        private void InvokeStateChanged(FunctionState state)
        {
            OnStateChanged?.Invoke(state);
        }

        #endregion

        public FunctionState State { get; private set; } = FunctionState.Initializing;

        protected virtual void EnableFunction()
        {
            ChangeFunctionState(FunctionState.Enabled);
        }

        public abstract void Run();

        protected virtual void DisableFunction()
        {
            ChangeFunctionState(FunctionState.Disabled);
        }

        private void ChangeFunctionState(FunctionState newState)
        {
            //same state
            if (State == newState)
            {
                return;
            }

            State = newState;

            //invoke event
            InvokeStateChanged(State);
        }
    }
}
