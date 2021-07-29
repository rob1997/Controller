using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMotionController : MotionController
{
    [Space]
    
    public MouseLook mouseLook;

    private PlayerInputActions _inputActions;

    public override void Initialize(Character character)
    {
        base.Initialize(character);
        
        if (GameManager.Instance.GetManager(out InputManager inputManager))
        {
            _inputActions = inputManager.InputActions;
        }
        
        _inputActions.Foot.Walk.started += delegate { TakeAction<SpeedRateAction>(SpeedRate.Walk); };
        _inputActions.Foot.Walk.canceled += delegate 
        {
            if (Rate == SpeedRate.Walk)
            {
                TakeAction<SpeedRateAction>(SpeedRate.Run);
            }
        };
        
        _inputActions.Foot.Sprint.started += delegate { TakeAction<SpeedRateAction>(SpeedRate.Sprint); };
        _inputActions.Foot.Sprint.canceled += delegate
        {
            if (Rate == SpeedRate.Sprint)
            {
                TakeAction<SpeedRateAction>(SpeedRate.Run);
            }
        };
        
        LookFrom = mouseLook.transform;
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

        Vector3 inputDirection = (realInput.x * right + realInput.y * forward).normalized;

        Velocity = new Vector3(inputDirection.x, Velocity.y, inputDirection.z);

        if (_inputActions.Foot.Jump.triggered)
        {
            TakeAction<JumpAction>();
        }
        
#if UNITY_EDITOR
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            TakeAction<LookModeAction>(CurrentLookMode == LookMode.Free ? LookMode.Strafe : LookMode.Free);
        }
#endif
    }

    public void Lock(Transform target, bool lockVertically = false)
    {
        LookAt = target;
        
        mouseLook.Target = target;
        mouseLook.LockVertically = lockVertically;
    }

    public void ClearLock()
    {
        LookAt = null;

        mouseLook.Target = null;
    }
}
