using System;
using UnityEngine;
using System.Collections;
using Unity.Mathematics;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

public class MouseLook : MonoBehaviour {
	 
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

	[FormerlySerializedAs("target")] [Space]
	public Transform testTarget;
	
	float _rotationX = 0F;
	float _rotationY = 0F;
	 
	private PlayerInputActions _inputActions;
	
	public Transform Target 
	{
		get => _target;

		set
		{
			if (value == null)
			{
				_lookAtTarget = false;
			}

			else
			{
				_lookAtTarget = true;
			}

			_target = value;
		} 
	}

	public bool HasTarget => Target != null;
	
	private Transform _target;
	
	private bool _lookAtTarget = false;

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
		if (GameManager.Instance.GetManager(out InputManager inputManager))
		{
			_inputActions = inputManager.InputActions;
		}
	}
	
	void Update()
	{
		if (Keyboard.current.xKey.wasPressedThisFrame)
		{
			Target = HasTarget ? null : testTarget;
		}
		
		Vector2 lookDelta = _inputActions.View.Look.ReadValue<Vector2>();
		
		// Read the mouse input axis
		_rotationX += _lookAtTarget && Target != null ? 0 : lookDelta.x * sensitivityX;
		_rotationY += lookDelta.y * sensitivityY;
		
		_rotationX = ClampAngle(_rotationX, minimumX, maximumX);
		_rotationY = ClampAngle(_rotationY, minimumY, maximumY);
			 
		Quaternion xQuaternion = Quaternion.AngleAxis(_rotationX, Vector3.up);
		Quaternion yQuaternion = Quaternion.AngleAxis(_rotationY, - Vector3.right);
		
		transform.rotation = xQuaternion * yQuaternion;

		if (_lookAtTarget)
		{
			LookAtTarget();
		}
	}

	private void LookAtTarget()
	{
		if (Target == null)
		{
			_lookAtTarget = false;
				
			return;
		}
			
		Vector3 forward = transform.forward;
		Vector3 position = transform.position;
			
		Vector3 targetPosition = Target.position;
			
		float toTargetAngleX = Vector3.SignedAngle(forward.GetXz(),
			(targetPosition - position).GetXz(), Vector3.up);

		_rotationX = Mathf.Lerp(_rotationX, _rotationX + toTargetAngleX, toTargetSensitivityX);
	}
	
	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F) angle += 360F;
		if (angle > 360F) angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}
}