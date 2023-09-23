using Character.Main;
using Core.Game;
using Core.Input;
using Core.Utils;
using Locomotion.Controllers;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.Controllers
{
    public class PlayerMotionController : MotionController
    {
        [field: SerializeField] public MouseLook MouseLook { get; private set; }

        private BaseInputActions _inputActions;
    
        private Transform _cameraTransform;

        public override void Initialize(Actor actor)
        {
            //disable CharacterController so position changes won't be reverted
            CharacterController.enabled = false;
            
            base.Initialize(actor);

            Character.Player player = (Character.Player) actor;
            
            //load position from file/saved file
            actor.transform.position = player.SerializedData.DataModel.Position.ToVector3;
            //revert back
            CharacterController.enabled = true;
            
            _inputActions = InputManager.Instance.InputActions;
        
            _inputActions.Foot.Walk.started += delegate { ChangeSpeedRate(SpeedRate.Walk); };
            _inputActions.Foot.Walk.canceled += delegate 
            {
                if (Rate == SpeedRate.Walk)
                {
                    ChangeSpeedRate(SpeedRate.Run);
                }
            };
        
            _inputActions.Foot.Sprint.started += delegate
            {
                ChangeSpeedRate(SpeedRate.Sprint);
            };
            _inputActions.Foot.Sprint.canceled += delegate
            {
                if (Rate == SpeedRate.Sprint)
                {
                    ChangeSpeedRate(SpeedRate.Run);
                }
            };
        
            LookFromTransform = MouseLook.transform;

            if (Camera.main != null) _cameraTransform = Camera.main.transform;
        
            else Debug.LogError("Please Assign Main Camera");
        }
    
        protected override void Update()
        {
            LevelBeamer();
            
            GetInput();
        
            base.Update();
        }

        //move beamer to player position level/plane
        //this is avoid targeting items between camera and player
        private void LevelBeamer()
        {
            Vector3 localBeamerPosition = LookFromTransform.InverseTransformPoint(Targeter.Muzzle.position);
            //arm length = half of height minus width of torso
            localBeamerPosition.z = (CharacterController.height / 2f) - CharacterController.radius;

            Targeter.Muzzle.position = LookFromTransform.TransformPoint(localBeamerPosition);
        }
        
        private void GetInput()
        {
            Vector2 realInput = _inputActions.Foot.Move.ReadValue<Vector2>();
        
            Vector3 forward = _cameraTransform.forward;
            Vector3 right = _cameraTransform.right;

            forward.y = 0;
            right.y = 0;

            Vector3 inputDirection = (realInput.x * right + realInput.y * forward).normalized;

            SetVelocity(new Vector3(inputDirection.x, Velocity.y, inputDirection.z));

            if (_inputActions.Foot.Jump.triggered)
            {
                TriggerJump();
            }
        
#if UNITY_EDITOR
            if (Keyboard.current.tabKey.wasPressedThisFrame)
            {
                ChangeLookMode(CurrentLookMode == LookMode.Free ? LookMode.Strafe : LookMode.Free);
            }
#endif
        }

        protected override void LookAtTarget()
        {
            base.LookAtTarget();

            if (MouseLook != null)
            {
                if (LookAtTransform != null)
                {
                    if (MouseLook.Target != LookAtTransform)
                    {
                        MouseLook.Target = LookAtTransform;
                        MouseLook.LockVertically = false;
                    }
                }

                else
                {
                    if (MouseLook.Target != null)
                    {
                        MouseLook.Target = null;
                    }
                }
            }
        }
    }
}
