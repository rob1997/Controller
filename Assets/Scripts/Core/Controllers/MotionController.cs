using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public abstract class MotionController : Controller
{
    [Serializable]
    public struct Speed
    {
        public float walk;
        public float run;
        public float sprint;
    }
    
    public enum SpeedRate
    {
        Walk,
        Run,
        Sprint
    }
    
    public enum LookMode
    {
        Free,
        Strafe,
    }
    
    public enum MotionMode
    {
        Static,
        Dynamic,
    }
    
    #region GroundStateChange

    public delegate void GroundStateChange(bool isGrounded);

    public event GroundStateChange OnGroundStateChange;

    private void InvokeGroundStateChange()
    {
        OnGroundStateChange?.Invoke(IsGrounded);
    }

    #endregion

    #region MotionModeChange

    public delegate void MotionModeChange(MotionMode mode);

    public event MotionModeChange OnMotionModeChange;

    private void InvokeMotionModeChange()
    {
        OnMotionModeChange?.Invoke(CurrentMotionMode);
    }

    #endregion
    
    #region LookModeChange

    public delegate void LookModeChange(LookMode mode);

    public event LookModeChange OnLookModeChange;

    private void InvokeLookModeChange()
    {
        OnLookModeChange?.Invoke(CurrentLookMode);
    }

    #endregion
    
    #region SpeedRateChange

    public delegate void SpeedRateChange(SpeedRate rate);

    public event SpeedRateChange OnSpeedRateChange;

    private void InvokeSpeedRateChange()
    {
        OnSpeedRateChange?.Invoke(Rate);
    }

    #endregion
    
    [SerializeField] private CharacterController characterController;
    
    [Space]
    
    [SerializeField] private Speed speed;
    
    [SerializeField] private float rotationSpeed;
    
    [Space]
    
    [SerializeField] private float jumpForce;
    
    [SerializeField] private float gravity = Physics.gravity.y;
    
    [SerializeField] protected bool controlAirMovement = false;
    
    [Space]
    
    [SerializeField] private Transform body;
    
    [SerializeField] private Transform lookAt;
    
    protected Vector3 Velocity;

    private Vector3 _cachedGroundedVelocity;
    
    private float _realSpeed;
    
    private float _verticalDistanceFromGround;
    
    private float _verticalDisplacementFromGround;

    /// <summary>
    /// extra force applied to keep grounded while moving on slopes and stairs
    /// </summary>
    private readonly float _extraDownForce = - 10000f;
    
    public bool IsGrounded { get; protected set; } = true;
    
    public bool IsJumping { get; protected set; }
    
    public SpeedRate Rate { get; protected set; } = SpeedRate.Run;
    
    public LookMode CurrentLookMode { get; protected set; } = LookMode.Free;
    
    public MotionMode CurrentMotionMode { get; protected set; } = MotionMode.Dynamic;
    
    public Transform LookAt => lookAt;
    
    public Transform LookFrom { get; protected set; }
    
    public override void Initialize(Character character)
    {
        base.Initialize(character);

        AddActions(new List<Action>
        {
            new SpeedRateAction(),
            new JumpAction(),
            new LookModeAction(),
        });
        
//        Physics.gravity = Vector3.up * gravity;
    }
    
    protected virtual void Update()
    {
        Look();
        
        CheckGround();

        ApplyGravity();
    }
    
    protected virtual void LateUpdate()
    {
        Move();
    }

    private void Look()
    {
        if (!IsGrounded && !controlAirMovement)
        {
            Velocity = new Vector3(_cachedGroundedVelocity.x, Velocity.y, _cachedGroundedVelocity.z);
        }

        Vector3 lookVelocity = new Vector3(Velocity.x, 0, Velocity.z);
        
        Quaternion lookRotation = body.rotation;
        
        switch (CurrentLookMode)
        {
            case LookMode.Free:
                if (lookVelocity.magnitude > 0)
                {
                    lookRotation = Quaternion.LookRotation(lookVelocity.normalized);
                }
                break;
            case LookMode.Strafe:
                Vector3 toTarget = lookAt.position - LookFrom.position;
                toTarget.y = 0;
                lookRotation = Quaternion.LookRotation(toTarget.normalized);
                if (Rate == SpeedRate.Sprint && lookVelocity.magnitude > 0)
                {
                    lookRotation = Quaternion.LookRotation(lookVelocity.normalized);
                }
                break;
        }
        
        body.rotation = Quaternion.RotateTowards(body.rotation, 
            lookRotation, rotationSpeed * 360f * Time.deltaTime);
    }

    private void CheckGround()
    {
        Transform controllerTransform = characterController.transform;
        
        Vector3 up = controllerTransform.up.normalized;
        
        Vector3 origin = controllerTransform.TransformPoint(characterController.center) + - up * characterController.height / 2f;

        float stepOffset = characterController.stepOffset + characterController.skinWidth;

        Debug.DrawLine(origin, origin - up * stepOffset, Color.red);
        
        if (IsGrounded)
        {
            bool cast = Physics.Raycast(origin, - up, stepOffset);

            IsGrounded = (characterController.isGrounded || cast) && Velocity.y <= 0;
            
            //take off/fall
            if (!IsGrounded)
            {
                _cachedGroundedVelocity = Velocity;
                
                InvokeGroundStateChange();
            }

            //always keep grounded
            else
            {
                //add extra force if grounded (not on edges/cast only)
                //useful for moving on slopes and stairs with low gravity
                if (cast) Velocity.y = _extraDownForce;

                else
                {
                    //apply minimum possible value to keep _characterController.isGrounded from fluctuating while actually grounded
                    Velocity.y = -.001f;
                }
            }
        }

        else
        {
            IsGrounded = characterController.isGrounded;
            
            //land
            if (IsGrounded)
            {
                InvokeGroundStateChange();
                
                _verticalDistanceFromGround = 0;

                _verticalDisplacementFromGround = 0;
            }
        }

        //check if vertical leap is from jump or not
        IsJumping = IsJumping && Velocity.y > 0;
    }

    private void ApplyGravity()
    {
        if (!IsGrounded)
        {
            float verticalFrameChange = gravity * Time.deltaTime;
            
            _verticalDistanceFromGround += Mathf.Abs(verticalFrameChange);

            //displacement depends if character is moving UP (Velocity.y > 0) or DOWN (Velocity.y <= 0)
            _verticalDisplacementFromGround += Velocity.y <= 0 ? - verticalFrameChange : verticalFrameChange;
            
            Velocity.y += verticalFrameChange;
        }
    }

    private void Move()
    {
        if (IsGrounded)
        {
            switch (Rate)
            {
                case SpeedRate.Walk:
                    _realSpeed = speed.walk;
                    break;
                case SpeedRate.Run:
                    _realSpeed = speed.run;
                    break;
                case SpeedRate.Sprint:
                    _realSpeed = speed.sprint;
                    break;
            }
        }

        Velocity.x *= _realSpeed;
        Velocity.z *= _realSpeed;

        characterController.Move(Velocity * Time.deltaTime);
    }

    #region Getters

    public Vector3 GetVelocity()
    {
        return Velocity;
    }
    
    public float GetSpeed()
    {
        switch (Rate)
        {
            case SpeedRate.Walk:
                return speed.walk;
            case SpeedRate.Run:
                return speed.run;
            case SpeedRate.Sprint:
                return speed.sprint;
            default:
                Debug.LogError("SpeedRate Not assigned");
                return 0;
        }
    }
    
    public float GetGravity()
    {
        return gravity;
    }
    
    public float GetVerticalDisplacement()
    {
        return _verticalDisplacementFromGround;
    }
    
    public float GetVerticalDistance()
    {
        return _verticalDistanceFromGround;
    }
    
    public float GetJumpForce()
    {
        return jumpForce;
    }

    public CharacterController GetCharacterController()
    {
        return characterController;
    }
    
    #endregion

    public void ChangeSpeedRate(SpeedRate rate)
    {
        if (Rate == rate) return;

        Rate = rate;
        
        InvokeSpeedRateChange();
    }
    
    public void ChangeLookMode(LookMode mode)
    {
        if (CurrentLookMode == mode) return;

        CurrentLookMode = mode;
        
        InvokeLookModeChange();
    }
    
    public void ChangeMotionMode(MotionMode mode)
    {
        if (CurrentMotionMode == mode) return;

        CurrentMotionMode = mode;
        
        InvokeMotionModeChange();
    }
    
    public void TriggerJump()
    {
        //restrict multiple jumps
        if (!IsGrounded) return;

        IsJumping = true;

        Velocity.y = jumpForce;
    }
}
