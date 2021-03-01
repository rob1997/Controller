using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Manager
{
    public PlayerInputActions InputActions { get; private set; }

    public override void Initialize()
    {
        InputActions = new PlayerInputActions();
        
        InputActions.Enable();
    }
}
