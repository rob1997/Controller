using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using NPC.Main;
using NPC.States;
using Sensors.Main;
using UnityEngine;

namespace NPC.States
{
    public class TargetDetectedTransition : StateTransition<LookState>
    {
        public Targeter Targeter { get; private set; }
        
        public override void InitializeTransition(LookState fromState)
        {
            base.InitializeTransition(fromState);
            
            Targeter = FromState.Controller.Actor.Targeter;
        }

        public override bool TryExitState()
        {
            TargetHit[] results = Targeter.FindTargets();

            //disable and run break function if vision sees something
            if (results.Length > 0 && results.Any(c =>
                    !FromState.IgnoreTagMask.Contains(c.Tag) && FromState.LookForLayerMask.HasLayer(c.Layer)))
            {
                return true;
            }

            return false;
        }
    }
}
