using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WalkAction : Action
{
    public override void OnAction()
    {
        ((MotionController) GetController()).ChangeSpeedRate(MotionController.SpeedRate.Walk);   
    }
}
