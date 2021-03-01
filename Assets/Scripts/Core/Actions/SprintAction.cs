using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SprintAction : Action
{
    public override void OnAction()
    {
        ((MotionController) GetController()).ChangeSpeedRate(MotionController.SpeedRate.Sprint);   
    }
}
