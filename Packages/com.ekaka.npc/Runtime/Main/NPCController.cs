using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character.Main;
using UnityEngine;

namespace NPC.Main
{
    public class NPCController : Controller
    {
        private StateBase[] _states = { };

        private StateBase[] EnabledStates => _states.Where(f => f.IsEnabled).ToArray();
        
        public override void Initialize(Actor actor)
        {
            base.Initialize(actor);

            StateBase[] states = GetComponentsInChildren<StateBase>();
        
            foreach (StateBase state in states)
            {
                if (AddState(state))
                {
                    state.Initialize(this);
                }
            }
        }

        protected bool AddState<T>(T state) where T : StateBase
        {
            Type type = state.GetType();
        
            //state already exists
            if (_states.Any(f => f.GetType() == type && f.IsUnique))
            {
                Debug.LogWarning($"can't add, {type.Name} already exists.");
            
                return false;
            }
        
            _states = _states.Append(state).ToArray();

            return true;
        }

        private void Update()
        {
            UpdateStates(StateUpdateType.Update);
            //update custom states in update to make update time more accurate
            UpdateStates(StateUpdateType.Custom);
        }
        
        private void FixedUpdate()
        {
            UpdateStates(StateUpdateType.FixedUpdate);
        }

        private void LateUpdate()
        {
            UpdateStates(StateUpdateType.LateUpdate);
        }

        private void UpdateStates(StateUpdateType updateType)
        {
            foreach (StateBase state in EnabledStates.Where(f => f.UpdateType == updateType))
            {
                if (updateType != StateUpdateType.Custom || state.StateUpdate.UpdateTime())
                {
                    state.UpdateState();
                    
                    state.TryExitState();
                    
                    if (state.IsCompleted && state.IsEnabled)
                    {
                        state.DisableState();
                    }
                }
            }
        }
    }
}
