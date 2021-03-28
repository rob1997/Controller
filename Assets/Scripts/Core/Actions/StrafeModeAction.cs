using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrafeModeAction : Action
{
    public override void OnAction()
    {
        ((MotionController) GetController()).ChangeMotionMode(MotionController.MotionMode.Strafe);
    }
}
