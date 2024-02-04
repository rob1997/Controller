using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC.Main
{
    public abstract class StateTransition<TFromState> : MonoBehaviour where TFromState : StateBase
    {
        [field: SerializeField] public StateBase[] ExitStates { get; private set; }
        
        public TFromState FromState { get; private set; }
        
        public virtual void InitializeTransition(TFromState fromState)
        {
            FromState = fromState;
        }
        
        public abstract bool TryExitState();
    }
}
