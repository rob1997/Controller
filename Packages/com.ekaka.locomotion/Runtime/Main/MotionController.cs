using System;
using Character.Main;
using Character.Damage;
using Core.Common;
using Locomotion.Common;
using Locomotion.Common.Grounder;
using Sensors.Main;
using UnityEngine;

namespace Locomotion.Main
{
    public abstract class MotionController : Controller
    {
        [Serializable]
        public struct Speed
        {
            [field: SerializeField] public float Walk { get; private set; }
            [field: SerializeField] public float Run { get; private set; }
            [field: SerializeField] public float Sprint { get; private set; }
            
            [field: Space]
            
            [field: SerializeField] public float Acceleration { get; private set; }
            [field: SerializeField] public float Deceleration { get; private set; }
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
    
        #region GroundStateChange

        public delegate void GroundStateChange(bool isGrounded);

        public event GroundStateChange OnGroundStateChange;

        private void InvokeGroundStateChange()
        {
            OnGroundStateChange?.Invoke(IsGrounded);
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
    
        [field: SerializeField] protected CharacterController CharacterController { get; private set; }
        
        [field: SerializeField] public Grounder Grounder { get; private set; }
        
        [field: Space]
    
        [field: SerializeField] public Speed LocomotionSpeed { get; private set; }
    
        [field: SerializeField] public float RotationSpeed { get; private set; }
    
        [field: Space]
    
        [field: SerializeField] public float JumpForce { get; private set; }
    
        [field: SerializeField] public float Gravity { get; private set; } = Physics.gravity.y;
    
        [field: SerializeField] public bool ControlAirMovement { get; private set; }
    
        [field: Space]
    
        [field: SerializeField] public Transform Body { get; private set; }
        
        [field: Tooltip("how far can the character see")]
        [field: SerializeField] public float VisionDistance { get; private set;  } = 100f;
    
        [field: Space]
        
        [field: SerializeField] public Transform Target { get; protected set; }
        
        [field: SerializeField] public Transform LookAtTransform { get; protected set; }

        public Vector3 Velocity => _velocity;
        
        public Vector3 RealVelocity => CharacterController.velocity;

        public float VerticalDistanceFromGround { get; private set; }
    
        public float VerticalDisplacementFromGround { get; private set; }
        
        public bool IsGrounded { get; protected set; } = true;
    
        public bool IsJumping { get; protected set; }
    
        public SpeedRate Rate { get; protected set; } = SpeedRate.Run;
    
        public LookMode CurrentLookMode { get; protected set; } = LookMode.Free;

        protected Targeter Targeter { get; private set; }
        
        protected Transform LookFromTransform { get; set; }
        
        private Vector3 _velocity;
        
        private Vector3 _cachedGroundedVelocity;
        
        private float _cachedStepOffset;
    
        private float _realSpeed;

        // normalized turn/rotate/look target value from - 1 to 1 (- 180 to 180 degrees)
        private float _turn;
    
        private RaycastHit _groundHit;

        private Animator _animator;

        private DamageController _damageController;

        // extra force applied to keep grounded while moving on slopes and stairs
        private readonly float _extraDownForce = - 10000f;
    
        public override void Initialize(Actor actor)
        {
            base.Initialize(actor);

            #region Grounder

            if (Grounder != null)
            {
                //get step height from character controller
                Grounder.maxStepHeight = CharacterController.stepOffset;

                //enable when on ground and disable when on air
                OnGroundStateChange += grounded => { Grounder.active = grounded; };
            }

            else Debug.LogWarning($"{nameof(Common.Grounder.Grounder)} component not found");

            #endregion

            #region Fall Damage

            //register fall damage
            if (actor.GetController(out _damageController))
                RegisterFallDamage();

            else
                Debug.LogWarning($"{nameof(DamageController)} not found under {nameof(Actor)} {Actor.name}");

            #endregion

            Targeter = actor.Targeter;
            
            LookFromTransform = Body.transform;

            #region Animation
            
            _animator = actor.Animator;

            if (_animator != null)
            {
                OnGroundStateChange += grounded =>
                {
                    _animator.ResetTrigger(grounded ? NameHashGroup.OnAirHash : NameHashGroup.OnLandHash);
                    
                    _animator.SetTrigger(grounded ?  NameHashGroup.OnLandHash : NameHashGroup.OnAirHash);
                };

                OnLookModeChange += mode =>
                {
                    switch (mode)
                    {
                        case LookMode.Free:
                    
                            _animator.ResetTrigger(NameHashGroup.OnStrafeMotionHash);
                            
                            _animator.SetTrigger(NameHashGroup.OnFreeMotionHash);
                    
                            break;
                        case LookMode.Strafe:
                    
                            _animator.ResetTrigger(NameHashGroup.OnFreeMotionHash);
                            
                            _animator.SetTrigger(NameHashGroup.OnStrafeMotionHash);
                    
                            break;
                    }
                };
            }
            
            else Debug.LogWarning($"{nameof(Animator)} component not found");

            #endregion
            
            Actor.Endurance.OnValueDepleted += delegate
            {
                if (Rate == SpeedRate.Sprint)
                {
                    ChangeSpeedRate(SpeedRate.Run);
                }
            };
        }
        
        private void RegisterFallDamage()
        {
            void Register()
            {
                OnGroundStateChange += grounded =>
                {
                    if (grounded)
                        _damageController.ApplyFallDamage(- _velocity.y);
                };
            };
                    
            if (_damageController.IsReady)
                Register();
                    
            else
                _damageController.OnReady += Register;
        }
        
        protected virtual void Update()
        {
            Look();
        
            LookAtTarget();
            
            CheckGround();

            ApplyGravity();
            
            if (_animator != null) AnimateMotion();
        }
    
        protected virtual void LateUpdate()
        {
            Move();
            
            // Apply after _velocity is set.
            ApplyStamina();
        }

        private void Look()
        {
            if (!IsGrounded && !ControlAirMovement)
            {
                _velocity = new Vector3(_cachedGroundedVelocity.x, _velocity.y, _cachedGroundedVelocity.z);
            }

            Vector3 lookVelocity = new Vector3(_velocity.x, 0, _velocity.z);
        
            Quaternion lookRotation = Body.rotation;
        
            switch (CurrentLookMode)
            {
                case LookMode.Free:
                    if (lookVelocity.magnitude > 0)
                    {
                        lookRotation = Quaternion.LookRotation(lookVelocity.normalized);
                    }
                    break;
                case LookMode.Strafe:
                    Vector3 toTarget = LookAtTransform != null ? LookAtTransform.position - LookFromTransform.position : LookFromTransform.forward;
                    toTarget.y = 0;
                    lookRotation = Quaternion.LookRotation(toTarget);
                    if (Rate == SpeedRate.Sprint && lookVelocity.magnitude > 0)
                    {
                        lookRotation = Quaternion.LookRotation(lookVelocity.normalized);
                    }
                    break;
            }

            Vector3 bodyForward = Body.forward;
            bodyForward.y = 0;
            bodyForward.Normalize();
            
            _turn = Vector3.SignedAngle(bodyForward, lookRotation * Vector3.forward, Body.up) / 180f;
            
            Body.rotation = Quaternion.RotateTowards(Body.rotation, 
                lookRotation, RotationSpeed * 360f * Time.deltaTime);
        }

        protected virtual void LookAtTarget()
        {
            Vector3 lookAtTarget;
            
            if (LookAtTransform != null)
            {
                lookAtTarget = LookAtTransform.position;
            }

            else
            {
                TargetHit[] hits = Targeter.FindTargets();
                    
                if (hits != null && hits.Length > 0)
                {
                    lookAtTarget = hits[0].Point;
                }

                else
                {
                    lookAtTarget = LookFromTransform.position + LookFromTransform.forward * VisionDistance;
                }
            }
                
            Target.position = lookAtTarget;
        }
        
        private void CheckGround()
        {
            Transform controllerTransform = CharacterController.transform;
        
            Vector3 up = controllerTransform.up.normalized;
        
            //cast origin
            Vector3 origin = controllerTransform.TransformPoint(CharacterController.center) + - up * CharacterController.height / 2f;

            float stepOffset = CharacterController.stepOffset + CharacterController.skinWidth;

            Debug.DrawLine(origin, origin - up * stepOffset, Color.red);
        
            if (IsGrounded)
            {
                bool cast = Physics.Raycast(origin, - up, out _groundHit, stepOffset);
                
                IsGrounded = (CharacterController.isGrounded || cast) 
                             //check player isn't jumping either
                             && _velocity.y <= 0;
            
                //take off/fall
                if (!IsGrounded)
                {
                    _cachedGroundedVelocity = _velocity;

                    _cachedStepOffset = CharacterController.stepOffset;

                    //avoid jitter-ing when jumping near edges
                    CharacterController.stepOffset = 0;
                    
                    InvokeGroundStateChange();
                }

                //always keep grounded
                else
                {
                    //add extra force if grounded (not on edges/cast only)
                    //useful for moving on slopes and stairs with low gravity
                    if (cast) _velocity.y = _extraDownForce;

                    else
                    {
                        //apply minimum possible value to keep _characterController.isGrounded from fluctuating while actually grounded
                        _velocity.y = -.001f;
                    }
                }
            }

            else
            {
                IsGrounded = CharacterController.isGrounded;
            
                //land
                if (IsGrounded)
                {
                    InvokeGroundStateChange();

                    //reapply cached value
                    CharacterController.stepOffset = _cachedStepOffset;
                    
                    VerticalDistanceFromGround = 0;

                    VerticalDisplacementFromGround = 0;
                }
            }

            //check if vertical leap is from jump or not
            IsJumping = IsJumping && _velocity.y > 0;
        }

        private void ApplyGravity()
        {
            if (!IsGrounded)
            {
                float verticalFrameChange = Gravity * Time.deltaTime;
            
                VerticalDistanceFromGround += Mathf.Abs(verticalFrameChange);

                //displacement depends if character is moving UP (Velocity.y > 0) or DOWN (Velocity.y <= 0)
                VerticalDisplacementFromGround += _velocity.y <= 0 ? - verticalFrameChange : verticalFrameChange;
            
                _velocity.y += verticalFrameChange;
            }
        }

        private void Move()
        {
            if (IsGrounded)
            {
                switch (Rate)
                {
                    case SpeedRate.Walk:
                        _realSpeed = LocomotionSpeed.Walk;
                        break;
                    case SpeedRate.Run:
                        _realSpeed = LocomotionSpeed.Run;
                        break;
                    case SpeedRate.Sprint:
                        _realSpeed = LocomotionSpeed.Sprint;
                        break;
                }
            }

            _velocity.x *= _realSpeed;
            _velocity.z *= _realSpeed;

            #region Slope Effect

            if (IsGrounded)
            {
                //xz velocity
                Vector3 horizontalDirection = _velocity;
                horizontalDirection.y = 0;
                horizontalDirection.Normalize();

                //moving?
                if (horizontalDirection.magnitude > 0)
                {
                    Vector3 normal = _groundHit.normal;
                    
                    //actual angle of slope
                    float angle = Vector3.Angle(normal, Vector3.up);
                    
                    normal.y = 0;
                
                    //up slope or down slope | - 1 to 1 value
                    float slopeDirection = Mathf.Cos(Vector3.Angle(horizontalDirection, normal) * Mathf.Deg2Rad);
                    
                    //normalized velocity component that is against moving up a slope
                    float normalizedCounterVelocity = 1f - Mathf.Cos(Mathf.Deg2Rad * angle);
                    
                    normalizedCounterVelocity *= slopeDirection;

                    _velocity.x += _velocity.x * normalizedCounterVelocity;
                    _velocity.z += _velocity.z * normalizedCounterVelocity;
                }
            }

            #endregion

            #region ac-de-Celeration

            float deltaTime = Time.deltaTime;
            
            //to reset interpolation speed
            float tX = deltaTime > 0 ? 1f / deltaTime : 0f;
            float tZ = deltaTime > 0 ? 1f / deltaTime : 0f;

            float realVelocityX = RealVelocity.x;
            float realVelocityZ = RealVelocity.z;
            
            float velocityX = _velocity.x;
            float velocityZ = _velocity.z;

            //X value
            if (realVelocityX < velocityX) tX = LocomotionSpeed.Acceleration;
            
            else if (realVelocityX > velocityX) tX = LocomotionSpeed.Deceleration;
            
            //Z value
            if (realVelocityZ < velocityZ) tZ = LocomotionSpeed.Acceleration;
            
            else if (realVelocityZ > velocityZ) tZ = LocomotionSpeed.Deceleration;
            
            //Interpolate Velocity
            _velocity.x = Mathf.Lerp(realVelocityX, velocityX, tX * deltaTime);
            
            _velocity.z = Mathf.Lerp(realVelocityZ, velocityZ, tZ * deltaTime);

            #endregion
            
            CharacterController.Move(_velocity * Time.deltaTime);
        }

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
    
        public void TriggerJump()
        {
            //restrict multiple jumps
            if (!IsGrounded) return;

            IsJumping = true;

            _velocity.y = JumpForce;
        }

        public void SetVelocity(Vector3 velocity)
        {
            _velocity = velocity;
        }

        private void ApplyStamina()
        {
            Vector3 velocityXz = new Vector3(_velocity.x, 0, _velocity.z);
            
            float normalizedDrainRate = Mathf.Clamp(velocityXz.magnitude / LocomotionSpeed.Sprint, 0f, 1f);
         
            if (Rate != SpeedRate.Sprint)
            {
                Actor.Endurance.RecoverStamina(1f - normalizedDrainRate);
            }
            
            else
            {
                Actor.Endurance.DrainStamina(normalizedDrainRate);
            }
        }
        
        #region Animate Motion

        private void AnimateMotion()
        {
            Vector3 velocityXz = new Vector3(RealVelocity.x, 0, RealVelocity.z);
        
            float forward = 0;
            float right = 0;
        
            switch (CurrentLookMode)
            {
                case LookMode.Free:
                
                    forward = velocityXz.magnitude;
                    right = 0;
                    break;
            
                case LookMode.Strafe:
                
                    Vector3 localVelocity = Body.InverseTransformDirection(RealVelocity);

                    forward = localVelocity.z;
                    right = localVelocity.x;
                    
                    break;
            }

            //adjust values to blend tree
            GetBlendValue(ref forward, ref right);
            
            _animator.SetFloat(NameHashGroup.ForwardHash, forward, NameHashGroup.DampTime, Time.deltaTime);

            _animator.SetFloat(NameHashGroup.RightHash, right, NameHashGroup.DampTime, Time.deltaTime);
        
            _animator.SetFloat(NameHashGroup.NormalizedVerticalDisplacementHash, - VerticalDisplacementFromGround / JumpForce);
            _animator.SetFloat(NameHashGroup.VerticalDistanceHash, VerticalDistanceFromGround);
        
            _animator.SetBool(NameHashGroup.IsGroundedHash, IsGrounded);
        }
        
        //0 - 1 is walk 1 - 2 is Run 2 - 3 is sprint
        private void GetBlendValue(ref float forward, ref float right)
        {
            void GetLimit(ref float value, out float initialBlendValue, out float lowerLimit, out float upperLimit)
            {
                value = Mathf.Abs(value);

                value = Mathf.Clamp(value, 0, LocomotionSpeed.Sprint);
                
                initialBlendValue = 0;
                
                lowerLimit = 0;
                upperLimit = 0;
                
                if (value <= LocomotionSpeed.Walk)
                {
                    upperLimit = LocomotionSpeed.Walk;
                }
            
                else if (value <= LocomotionSpeed.Run)
                {
                    initialBlendValue = NameHashGroup.WalkBlendValue;
                
                    lowerLimit = LocomotionSpeed.Walk;
                    upperLimit = LocomotionSpeed.Run;
                }
            
                else if (value <= LocomotionSpeed.Sprint)
                {
                    initialBlendValue = NameHashGroup.RunBlendValue;
                
                    lowerLimit = LocomotionSpeed.Run;
                    upperLimit = LocomotionSpeed.Sprint;
                }
            }
            
            //forward
            float signZ = Mathf.Sign(forward);
            
            GetLimit(ref forward, out float initialBlendValueZ, out float lowerLimitZ, out float upperLimitZ);
            
            forward = initialBlendValueZ + Utils.NormalizeValue(forward, lowerLimitZ, upperLimitZ);

            forward *= signZ;
            
            //right
            float signX = Mathf.Sign(right);
            
            GetLimit(ref right, out float initialBlendValueX, out float lowerLimitX, out float upperLimitX);
            
            right = initialBlendValueX + Utils.NormalizeValue(right, lowerLimitX, upperLimitX);

            right *= signX;
        }

        #endregion
    }
}
