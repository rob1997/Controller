using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpAction : Action
{
    public override void OnAction()
    {
        ((MotionController) GetController()).TriggerJump();   
    }
}
