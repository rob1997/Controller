using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class AnimationController : Controller
{
    [SerializeField] private Animator animator;

    private MotionController _motionController;
    
    private PlayerInputActions _inputActions;

    public override void Initialize(Character character)
    {
        base.Initialize(character);
        
        if (GameManager.Instance.GetManager(out InputManager inputManager))
        {
            _inputActions = inputManager.InputActions;
        }
        
        character.GetController(out _motionController);

        _motionController.OnGroundStateChange += grounded =>
        {
            animator.ResetTrigger(grounded ? Constants.OnAirHash : Constants.OnGroundedHash);
            animator.SetTrigger(grounded ?  Constants.OnGroundedHash : Constants.OnAirHash);
        };

        _motionController.OnLookModeChange += mode =>
        {
            switch (mode)
            {
                case MotionController.LookMode.Free:
                    animator.ResetTrigger(Constants.OnStrafeMotionHash);
                    animator.SetTrigger(Constants.OnFreeMotionHash);
                    break;
                case MotionController.LookMode.Strafe:
                    animator.ResetTrigger(Constants.OnFreeMotionHash);
                    animator.SetTrigger(Constants.OnStrafeMotionHash);
                    break;
            }
        };
    }

    private void Update()
    {
        Vector3 realVelocity = _motionController.GetVelocity();
        Vector2 realInput = _inputActions.Foot.Move.ReadValue<Vector2>();
        
        float speed = new Vector3(realVelocity.x, 0, realVelocity.z).normalized.magnitude;

        float forward = 0;
        float right = 0;
        
        switch (_motionController.CurrentLookMode)
        {
            case MotionController.LookMode.Free:
                forward = speed;
                right = 0;
                break;
            case MotionController.LookMode.Strafe:
                forward = realInput.y * speed;
                right = realInput.x * speed;
                break;
        }
        
        animator.SetFloat(Constants.ForwardHash, forward, .15f, Time.deltaTime);

        animator.SetFloat(Constants.RightHash, right, .15f, Time.deltaTime);
        
        animator.SetInteger(Constants.RawSpeedHash, Mathf.RoundToInt(speed));
        animator.SetFloat(Constants.SpeedHash, speed * GetSpeedRate(), .15f, Time.deltaTime);
        
        animator.SetFloat(Constants.VerticalDisplacementHash, - _motionController.GetVerticalDisplacement() / _motionController.GetJumpForce());
        
        animator.SetBool(Constants.IsGroundedHash, _motionController.IsGrounded);
    }

    private float GetSpeedRate()
    {
        switch (_motionController.Rate)
        {
            case MotionController.SpeedRate.Walk:
                return .5f;
            case MotionController.SpeedRate.Run:
                return 1f;
            case MotionController.SpeedRate.Sprint:
                return 2f;
            default:
                Debug.LogError("All cases should be handled");
                return 0;
        }
    }
    
    public Animator GetAnimator()
    {
        return animator;
    }
}
