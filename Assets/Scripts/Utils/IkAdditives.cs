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

    private float _cachedWeight;
    
    private void Start()
    {
        _grounderBipedIk = GetComponent<GrounderBipedIK>();

        motionController.OnGroundStateChange += grounded =>
        {
            if (grounded)
            {
                _grounderBipedIk.weight = _cachedWeight;
            }

            else
            {
                _cachedWeight = _grounderBipedIk.weight;

                _grounderBipedIk.weight = 0;
            }
        };
    }
}
