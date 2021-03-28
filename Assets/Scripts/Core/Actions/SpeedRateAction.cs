using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedRateAction : Action
{
    public override void OnAction(params object[] objs)
    {
        ((MotionController) GetController()).ChangeSpeedRate(GetObj<MotionController.SpeedRate>(objs[0]));
    }
}
