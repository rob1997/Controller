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
        private State[] _states = { };

        private State[] EnabledStates => _states.Where(f => f.Status == StateStatus.Enabled).ToArray();
        
        public override void Initialize(Actor actor)
        {
            base.Initialize(actor);

            State[] states = GetComponentsInChildren<State>();
        
            foreach (State state in states)
            {
                if (AddState(state))
                {
                    state.Initialize(this);
                }
            }
        }

        protected bool AddState<T>(T state) where T : State
        {
            Type type = state.GetType();
        
            //state already exists
            if (_states.Any(f => f.GetType() == type && f.IsUnique))
            {
                Debug.LogWarning($"can't add, {type.Name} is a unique {nameof(State)}");
            
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
            foreach (State state in EnabledStates.Where(f => f.StateUpdate.UpdateType == updateType))
            {
                switch (updateType)
                {
                    case StateUpdateType.Custom:
                        
                        if (state.StateUpdate.UpdateTime())
                        {
                            state.UpdateState();
                            //check if state is completed/update frequency times
                            if (state.StateUpdate.Completed)
                            {
                                state.CompleteFunction();
                            }
                        }
                        
                        break;
                
                    default:
                        
                        state.UpdateState();
                        
                        break;
                }
            }
        }
    }
}
