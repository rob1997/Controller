using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Serialization;

public class IkAdditives : MonoBehaviour
{
    [SerializeField] private MotionController motionController;
    
    private GrounderBipedIK _grounderBipedIk;
    
    private void Start()
    {
        _grounderBipedIk = GetComponent<GrounderBipedIK>();
    }

    private void Update()
    {
        _grounderBipedIk.weight = motionController.IsGrounded ? 1f : 0f;
    }
}
