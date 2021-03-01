using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RunAction : Action
{
    public override void OnAction()
    {
        ((MotionController) GetController()).ChangeSpeedRate(MotionController.SpeedRate.Run);   
    }
}
