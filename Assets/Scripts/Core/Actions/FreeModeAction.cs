using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FreeModeAction : Action
{
    public override void OnAction()
    {
        ((MotionController) GetController()).ChangeMotionMode(MotionController.MotionMode.Free);
    }
}
