using Core.Game;
using UnityEngine;
using Core.Input;

namespace Core.Utils
{
    public class MouseLook : MonoBehaviour
    {
        public float sensitivityX = 15F;
        public float sensitivityY = 15F;
	
        [Space]
	    
        public float toTargetSensitivityX = .05f;
        public float toTargetSensitivityY = .05f;
	    
        [Space]
	
        public float minimumX = -360F;
        public float maximumX = 360F;
	 
        [Space]
	
        public float minimumY = -60F;
        public float maximumY = 60F;

        float _rotationX = 0F;
        float _rotationY = 0F;

        private BaseInputActions _inputActions;
	
        private Transform _target;
        
        private bool _lockOnTarget = false;

        /// <summary>
        /// 
        /// </summary>
        public Vector2 Gain { get; set; }
        
        public Transform Target 
        {
            get => _target;

            set
            {
                if (value == null)
                {
                    _lockOnTarget = false;
                }

                else
                {
                    _lockOnTarget = true;
                }

                _target = value;
            } 
        }
	
        public bool HasTarget => Target != null;

        public bool LockVertically { get; set; }

        protected virtual void Start()
        {
            if (GameManager.Instance.IsReady)
            {
                Initialize();
            }

            else
            {
                GameManager.Instance.OnReady += Initialize;
            }
        }
	
        void Initialize()
        {
            _inputActions = InputManager.Instance.InputActions;
        }
	
        void Update()
        {
            Vector2 lookDelta = _inputActions.View.Look.ReadValue<Vector2>();

            if (_lockOnTarget)
            {
                Vector3 lookAtDirection = _target.position - transform.position;

                float xTarget = Quaternion.LookRotation(lookAtDirection).eulerAngles.y;

                _rotationX = Mathf.LerpAngle(_rotationX, xTarget, toTargetSensitivityX);

                if (LockVertically)
                {
                    float yTarget = - Quaternion.LookRotation(lookAtDirection).eulerAngles.x;
                
                    _rotationY = Mathf.LerpAngle(_rotationY, yTarget, toTargetSensitivityY);
                }

                else
                {
                    _rotationY += lookDelta.y * sensitivityY;
                }
            }

            else
            {
                // Read the mouse input axis
                _rotationX += lookDelta.x * sensitivityX;
                _rotationY += lookDelta.y * sensitivityY;
            }

            _rotationX += Gain.x;
            _rotationY += Gain.y;
            
            _rotationX = Utils.ClampAngle(_rotationX, minimumX, maximumX);
            _rotationY = Utils.ClampAngle(_rotationY, minimumY, maximumY);
			 
            Quaternion xQuaternion = Quaternion.Euler(Vector3.up * _rotationX);
            Quaternion yQuaternion = Quaternion.Euler(- Vector3.right * _rotationY);
		
            transform.rotation = xQuaternion * yQuaternion;
        }
    }
}