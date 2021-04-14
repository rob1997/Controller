using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MotionController : Controller
{
    public enum SpeedRate
    {
        Walk,
        Run,
        Sprint
    }
    
    public enum MotionMode
    {
        Free,
        Strafe,
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
        OnMotionModeChange?.Invoke(Mode);
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
    
    [SerializeField] private float speed;
    
    [SerializeField] private float rotationSpeed;
    
    [Space]
    
    [SerializeField] private float jumpHeight;
    
    [SerializeField] private float gravity = Physics.gravity.y;
    
    [SerializeField] protected bool controlAirMovement = false;
    
    [Space]
    
    [SerializeField] private Transform body;
    
    [SerializeField] private Transform lookAt;
    
    protected Vector3 Velocity;

    private CharacterController _characterController;

    private Vector3 _cachedGroundedVelocity;
    
    private float _realSpeed;
    
    private float _verticalDistanceFromGround;
    
    private float _verticalDisplacementFromGround;

    /// <summary>
    /// extra force applied to keep grounded while moving on slopes and stairs
    /// </summary>
    private readonly float _extraDownForce = Physics.gravity.y;
    
    public bool IsJumping { get; private set; }

    public bool IsGrounded { get; protected set; } = true;
    
    public SpeedRate Rate { get; protected set; } = SpeedRate.Run;
    
    public MotionMode Mode { get; protected set; } = MotionMode.Free;
    
    public Transform LookAt => lookAt;
    
    public Transform LookFrom { get; protected set; }
    
    public override void Initialize(Character character)
    {
        base.Initialize(character);

        AddActions(new List<Action>
        {
            new SpeedRateAction(),
            new JumpAction(),
            new MotionModeAction(),
        });
        
        _characterController = character.GetComponent<CharacterController>();
    }
    
    protected virtual void Update()
    {
        Look();
        
        CheckGround();
        
        ApplyGravity();
        
        if (IsJumping)
        {
            Jump();
        }
    }
    
    protected virtual void LateUpdate()
    {
        Move();
    }

    private void Look()
    {
        if (!controlAirMovement && !IsGrounded)
        {
            Velocity = _cachedGroundedVelocity;
        }

        Vector3 lookVelocity = new Vector3(Velocity.x, 0, Velocity.z);
        
        Quaternion lookRotation = body.rotation;
        
        switch (Mode)
        {
            case MotionMode.Free:
                if (lookVelocity.magnitude > 0)
                {
                    lookRotation = Quaternion.LookRotation(lookVelocity.normalized);
                }
                break;
            case MotionMode.Strafe:
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
        Vector3 origin = transform.position + Vector3.down * _characterController.height / 2f;

        if (IsGrounded)
        {
            bool cast = Physics.Raycast(origin, Vector3.down, out RaycastHit hit, _characterController.stepOffset);
            
            IsGrounded = _characterController.isGrounded
                         || cast;
            
            //take off/fall
            if (!IsGrounded)
            {
                _cachedGroundedVelocity = Velocity;
                
                InvokeGroundStateChange();
            }

            //always keep grounded
            else
            {
                //add extra force if grounded
                //useful for moving on slopes and stairs with low gravity
                Velocity.y += _extraDownForce;
            }
        }

        else
        {
            IsGrounded = _characterController.isGrounded;

            //land
            if (IsGrounded)
            {
                _verticalDistanceFromGround = 0;

                _verticalDisplacementFromGround = 0;
                
                InvokeGroundStateChange();
            }
        }

        if (!IsGrounded && !IsJumping)
        {
            _verticalDistanceFromGround += Mathf.Abs(gravity) * Time.deltaTime;
            
            _verticalDisplacementFromGround += gravity * Time.deltaTime;
        }
    }
    
    private void ApplyGravity()
    {
        Velocity.y = gravity;
    }
    
    private void Jump()
    {
        Velocity.y = Mathf.Abs(gravity);
            
        _verticalDistanceFromGround += Mathf.Abs(gravity) * Time.deltaTime;

        _verticalDisplacementFromGround = _verticalDistanceFromGround;
        
        //jumpHeight reached
        if (_verticalDistanceFromGround >= jumpHeight)
        {
            IsJumping = false;
        }
    }
    
    private void Move()
    {
        if (IsGrounded)
        {
            switch (Rate)
            {
                case SpeedRate.Walk:
                    _realSpeed = speed / 2.5f;
                    break;
                case SpeedRate.Run:
                    _realSpeed = speed;
                    break;
                case SpeedRate.Sprint:
                    _realSpeed = speed * 1.5f;
                    break;
            }
        }

        Velocity.x *= _realSpeed;
        Velocity.z *= _realSpeed;

        _characterController.Move(Velocity * Time.deltaTime);
    }
    
    public Vector3 GetVelocity()
    {
        return Velocity;
    }
    
    public float GetSpeed()
    {
        return speed;
    }
    
    public float GetGravity()
    {
        return gravity;
    }
    
    public float GetVerticalDisplacement()
    {
        return _verticalDisplacementFromGround;
    }

    public void ChangeSpeedRate(SpeedRate rate)
    {
        if (Rate == rate) return;

        Rate = rate;
        
        InvokeSpeedRateChange();
    }
    
    public void ChangeMotionMode(MotionMode mode)
    {
        if (Mode == mode) return;

        Mode = mode;
        
        InvokeMotionModeChange();
    }

    public void TriggerJump()
    {
        //restrict multiple jumps
        if (!IsGrounded) return;

        IsJumping = true;
    }
}
