using System;
using UnityEngine;
using System.Collections;

public class MouseLook : MonoBehaviour {
	 
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;
	 
	[Space]
	
	public float minimumX = -360F;
	public float maximumX = 360F;
	 
	[Space]
	
	public float minimumY = -60F;
	public float maximumY = 60F;

	[Tooltip("If true gameObject won't rotate together with parent when parent does")]
	[Space] public bool parentIndependent = true;
	
	float _rotationX = 0F;
	float _rotationY = 0F;
	 
	private Quaternion _initialLocalRotation;
	private Quaternion _initialRotation;
	
	private PlayerInputActions _inputActions;
	
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
		
		_initialLocalRotation = transform.localRotation;
		_initialRotation = transform.rotation;
	}
	
	void Update()
	{
		Vector2 lookDelta = _inputActions.Foot.Look.ReadValue<Vector2>();
		
		// Read the mouse input axis
		_rotationX += lookDelta.x * sensitivityX;
		_rotationY += lookDelta.y * sensitivityY;
			 
		_rotationX = ClampAngle(_rotationX, minimumX, maximumX);
		_rotationY = ClampAngle(_rotationY, minimumY, maximumY);
			 
		Quaternion xQuaternion = Quaternion.AngleAxis(_rotationX, Vector3.up);
		Quaternion yQuaternion = Quaternion.AngleAxis(_rotationY, -Vector3.right);

		if (parentIndependent)
		{
			_initialRotation = _initialLocalRotation * xQuaternion * yQuaternion;
		}

		else
		{
			transform.localRotation = _initialLocalRotation * xQuaternion * yQuaternion;
		}
	}

	private void LateUpdate()
	{
		if (parentIndependent)
		{
			transform.rotation = _initialRotation;
		}
	}

	public static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360F)
			angle += 360F;
		if (angle > 360F)
			angle -= 360F;
		return Mathf.Clamp(angle, min, max);
	}
}