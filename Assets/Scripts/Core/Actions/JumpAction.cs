﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpAction : Action
{
    public override void OnAction(params object[] objs)
    {
        ((MotionController) GetController()).TriggerJump();
    }
}
