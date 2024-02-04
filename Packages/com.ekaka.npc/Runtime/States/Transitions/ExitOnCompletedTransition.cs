using System.Collections;
using System.Collections.Generic;
using NPC.Main;
using UnityEngine;

namespace NPC.States
{
    public class ExitOnCompletedTransition<T> : StateTransition<T> where T : State<T>
    {
        public override bool TryExitState()
        {
            return FromState.IsCompleted;
        }
    }
}
