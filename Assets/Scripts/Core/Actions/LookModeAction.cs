using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookModeAction : Action
{
    public override void OnAction(params object[] objs)
    {
        ((MotionController) GetController()).ChangeLookMode(GetObj<MotionController.LookMode>(objs[0]));
    }
}
