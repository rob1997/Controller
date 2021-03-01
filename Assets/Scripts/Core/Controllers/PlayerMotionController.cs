using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMotionController : MotionController
{
    private PlayerInputActions _inputActions;

    public override void Initialize(Character character)
    {
        base.Initialize(character);
        
        if (GameManager.Instance.GetManager(out InputManager inputManager))
        {
            _inputActions = inputManager.InputActions;
        }
        
        _inputActions.Foot.Walk.started += delegate { TakeAction<WalkAction>(); };
        _inputActions.Foot.Walk.canceled += delegate 
        {
            if (Rate == SpeedRate.Walk)
            {
                TakeAction<RunAction>();
            }
        };
        
        _inputActions.Foot.Sprint.started += delegate { TakeAction<SprintAction>(); };
        _inputActions.Foot.Sprint.canceled += delegate
        {
            if (Rate == SpeedRate.Sprint)
            {
                TakeAction<RunAction>();
            }
        };
    }
    
    protected override void Update()
    {
        GetInput();
        
        base.Update();
    }

    private void GetInput()
    {
        Vector2 realInput = _inputActions.Foot.Move.ReadValue<Vector2>();
        
        Transform cameraTransform = Camera.main.transform;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0;
        right.y = 0;

        Velocity = (realInput.x * right + realInput.y * forward).normalized;
    }
}
