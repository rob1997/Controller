using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class Ragdoll : MonoBehaviour
{
    [SerializeField] private PuppetMaster puppetMaster;
    [SerializeField] private BehaviourPuppet behaviourPuppet;
    
    [Space]
    
    [SerializeField] private PuppetMaster.StateSettings stateSettings = PuppetMaster.StateSettings.Default;

    [Space]
    
    [SerializeField] private int ragdollLayer;
    [SerializeField] private int pushableLayer;

    private void Start()
    {
        behaviourPuppet.onRegainBalance.unityEvent.AddListener(delegate
        {
            puppetMaster.mode = PuppetMaster.Mode.Kinematic;

            puppetMaster.muscleWeight = 1;
            
            foreach (Muscle muscle in puppetMaster.muscles)
            {
                muscle.rigidbody.interpolation = RigidbodyInterpolation.None;
                
                muscle.rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
            
            Physics.IgnoreLayerCollision(ragdollLayer, pushableLayer, true);
        });
    }

    private void Update()
    {
        if (Keyboard.current.numpad0Key.wasPressedThisFrame)
        {
            Activate();
        }
        
        else if (Keyboard.current.numpad1Key.wasPressedThisFrame)
        {
            Deactivate();
        }
    }

    private void FixedUpdate()
    {
        if (puppetMaster.state == PuppetMaster.State.Dead && puppetMaster.mode == PuppetMaster.Mode.Active)
        {
            foreach (Muscle muscle in puppetMaster.muscles)
            {
                muscle.rigidbody.AddForce(- 30f * muscle.rigidbody.mass * Vector3.up);
            }
        }
    }

    public void Activate()
    {
        puppetMaster.Kill(stateSettings);

        foreach (Muscle muscle in puppetMaster.muscles)
        {
            muscle.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

            muscle.rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            
            muscle.rigidbody.useGravity = false;
        }
        
        Physics.IgnoreLayerCollision(ragdollLayer, pushableLayer, false);
    }
    
    public void Deactivate()
    {
        puppetMaster.muscleWeight = 0;
        
        puppetMaster.Resurrect();
    }
}