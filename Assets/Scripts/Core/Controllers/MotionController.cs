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
    
    protected Vector3 Velocity;

    private CharacterController _characterController;

    [SerializeField] private float speed;
    
    [SerializeField] private float rotationSpeed;
    
    [SerializeField] private Transform body;
    
    public bool IsGrounded { get; protected set; }

    public SpeedRate Rate { get; protected set; } = SpeedRate.Run;
    
    public override void Initialize(Character character)
    {
        base.Initialize(character);

        AddActions(new List<Action>
        {
            new WalkAction(),
            new RunAction(),
            new SprintAction()
        });
        
        _characterController = character.GetComponent<CharacterController>();
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
        if (IsGrounded && Velocity.magnitude > 0)
        {
            body.transform.rotation = Quaternion.RotateTowards(body.transform.rotation, 
                Quaternion.LookRotation(Velocity.normalized), rotationSpeed * 360f * Time.deltaTime);
        }
    }
    
    private void CheckGround()
    {
        Vector3 origin = transform.position + Vector3.down * _characterController.height / 2f;

        if (IsGrounded)
        {
            IsGrounded = _characterController.isGrounded
                         || Physics.Raycast(origin, Vector3.down, _characterController.stepOffset);
        }

        else
        {
            IsGrounded = _characterController.isGrounded;
        }
    }
    
    private void ApplyGravity()
    {
        Velocity += Physics.gravity;
    }
    
    private void Move()
    {
        float localSpeed = 0;
        
        if (IsGrounded)
        {
            switch (Rate)
            {
                case SpeedRate.Walk:
                    localSpeed = speed / 2.5f;
                    break;
                case SpeedRate.Run:
                    localSpeed = speed;
                    break;
                case SpeedRate.Sprint:
                    localSpeed = speed * 1.5f;
                    break;
            }
        }
        
        Velocity.x *= localSpeed;
        Velocity.z *= localSpeed;
        
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

    public void ChangeSpeedRate(SpeedRate rate)
    {
        if (Rate == rate)
        {
            Debug.LogError($"SpeedRate already {rate}");
            
            return;
        }

        Rate = rate;
    }
}
