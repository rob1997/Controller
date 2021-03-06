﻿using System;
using UnityEngine;
using System.Collections;

public class MouseLook : MonoBehaviour {
	 
	public float sensitivityX = 15F;
	public float sensitivityY = 15F;
	 
	public float minimumX = -360F;
	public float maximumX = 360F;
	 
	public float minimumY = -60F;
	public float maximumY = 60F;
	 
	float _rotationX = 0F;
	float _rotationY = 0F;
	 
	Quaternion _originalRotation;
	
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
		
		_originalRotation = transform.localRotation;
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
			 
		transform.localRotation = _originalRotation * xQuaternion * yQuaternion;
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