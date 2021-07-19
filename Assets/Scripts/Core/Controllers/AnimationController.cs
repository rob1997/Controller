using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class AnimationController : Controller
{
    #region StateCompleted

    public delegate void StateCompleted(AnimatorStateInfo stateInfo);

    public event StateCompleted OnStateCompleted;

    private void InvokeStateCompleted(AnimatorStateInfo stateInfo)
    {
        OnStateCompleted?.Invoke(stateInfo);
    }

    #endregion

    #region StateStarted

    public delegate void StateStarted(AnimatorStateInfo stateInfo);

    public event StateStarted OnStateStarted;

    private void InvokeStateStarted(AnimatorStateInfo stateInfo)
    {
        OnStateStarted?.Invoke(stateInfo);
    }

    #endregion
    
    [SerializeField] private Animator animator;
    
    [Space]
    
    [SerializeField] private bool trackAnimatorStates;

    private MotionController _motionController;
    
    private PlayerInputActions _inputActions;

    private AnimatorStateInfo[] _currentStates;
    
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
            animator.ResetTrigger(grounded ? Constants.Animation.OnAirHash : Constants.Animation.OnLandHash);
            animator.SetTrigger(grounded ?  Constants.Animation.OnLandHash : Constants.Animation.OnAirHash);
        };

        _motionController.OnLookModeChange += mode =>
        {
            switch (mode)
            {
                case MotionController.LookMode.Free:
                    animator.ResetTrigger(Constants.Animation.OnStrafeMotionHash);
                    animator.SetTrigger(Constants.Animation.OnFreeMotionHash);
                    break;
                case MotionController.LookMode.Strafe:
                    animator.ResetTrigger(Constants.Animation.OnFreeMotionHash);
                    animator.SetTrigger(Constants.Animation.OnStrafeMotionHash);
                    break;
            }
        };
        
        _currentStates = new AnimatorStateInfo[animator.layerCount];
        
        for (int i = 0; i < animator.layerCount; i++)
        {
            _currentStates[i] = animator.GetCurrentAnimatorStateInfo(i);
        }
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
        
        animator.SetFloat(Constants.Animation.ForwardHash, forward, .15f, Time.deltaTime);

        animator.SetFloat(Constants.Animation.RightHash, right, .15f, Time.deltaTime);
        
        animator.SetInteger(Constants.Animation.RawSpeedHash, Mathf.RoundToInt(speed));
        animator.SetFloat(Constants.Animation.SpeedHash, speed * GetSpeedRate(), .15f, Time.deltaTime);
        
        animator.SetFloat(Constants.Animation.NormalizedVerticalDisplacementHash, - _motionController.GetVerticalDisplacement() / _motionController.GetJumpForce());
        animator.SetFloat(Constants.Animation.VerticalDistanceHash, _motionController.GetVerticalDistance());
        
        animator.SetBool(Constants.Animation.IsGroundedHash, _motionController.IsGrounded);
        
        if (trackAnimatorStates)
        {
            UpdateStates();
        }
    }

    private void UpdateStates()
    {
        for (int i = 0; i < animator.layerCount; i++)
        {
            if (animator.GetCurrentAnimatorStateInfo(i).fullPathHash != _currentStates[i].fullPathHash)
            {
                if (Constants.Animation.HasState(_currentStates[i].shortNameHash)) InvokeStateCompleted(_currentStates[i]);
                
                _currentStates[i] = animator.GetCurrentAnimatorStateInfo(i);
                
                if (Constants.Animation.HasState(_currentStates[i].shortNameHash)) InvokeStateStarted(_currentStates[i]);
            }
        }
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
