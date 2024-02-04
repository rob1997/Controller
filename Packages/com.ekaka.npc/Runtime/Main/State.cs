using UnityEngine;

namespace NPC.Main
{
    public abstract class State<T> : StateBase where T : State<T>
    {
        [field: SerializeField] public StateTransition<T>[] Transitions { get; private set; }
        
        public override void Initialize(NPCController controller)
        {
            base.Initialize(controller);
            
            foreach (var transition in Transitions)
            {
                transition.InitializeTransition(this as T);
            }
            
            if (EnableOnInitialize)
            {
                EnableState();
            }
        }

        public override void TryExitState()
        {
            //can't exit function from a different state
            if (!IsEnabled)
            {
                Debug.LogWarning($"can't exit while {GetType().Name} is disabled");
                
                return;
            }

            IsCompleted = IsCompleted || (UpdateType == StateUpdateType.Custom && StateUpdate.Completed);
            
            foreach (var transition in Transitions)
            {
                if (transition.TryExitState())
                {
                    if (IsEnabled)
                    {
                        DisableState();
                    }
            
                    //check and enable exit states
                    if (transition.ExitStates != null)
                    {
                        foreach (var exitState in transition.ExitStates)
                        {
                            exitState.EnableState();
                        }
                    }
                }
            }
        }
    }
}