using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class AnimationController : Controller
{
    [SerializeField] private Animator animator;

    private MotionController _motionController;
    
    public override void Initialize(Character character)
    {
        base.Initialize(character);
        
        character.GetController(out _motionController);
    }

    private void Update()
    {
        Vector3 realVelocity = _motionController.GetVelocity();

        float speed = new Vector3(realVelocity.x, 0, realVelocity.z).normalized.magnitude;

        //realVelocity = _motionController.GetTarget().InverseTransformDirection(realVelocity);
        realVelocity.y = 0;
        
        float forward = realVelocity.z;
        float right = realVelocity.x;
        
        animator.SetFloat(Constants.ForwardHash, forward);
        animator.SetFloat(Constants.RightHash, right);
        
        animator.SetInteger(Constants.RawSpeedHash, Mathf.RoundToInt(speed));
        animator.SetFloat(Constants.SpeedHash, speed * GetSpeedRate(), .15f, Time.deltaTime);
        
        animator.SetFloat(Constants.VerticalDisplacementHash, _motionController.GetVerticalDisplacement());
        
        animator.SetBool(Constants.IsGroundedHash, _motionController.IsGrounded);
    }

    public float GetSpeedRate()
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
