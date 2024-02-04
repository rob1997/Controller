using System;
using System.Collections;
using System.Collections.Generic;
using NPC.Main;
using UnityEngine;

namespace NPC.States
{
    public class TargetOutOfSightTransition : StateTransition<TargetState>
    {
        public override bool TryExitState()
        {
            //if timeout is reached disable function
            return FromState.TimeSinceLastSeen > FromState.LastSeenTimeout;
        }
    }
}
