using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionModeAction : Action
{
    public override void OnAction(params object[] objs)
    {
        ((MotionController) GetController()).ChangeMotionMode(GetObj<MotionController.MotionMode>(objs[0]));
    }
}
