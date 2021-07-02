using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;

public class Ragdoll : MonoBehaviour
{
    public PuppetMaster puppetMaster;
    [SerializeField] private BehaviourPuppet behaviourPuppet;
    
    [Space]
    
    [SerializeField] private PuppetMaster.StateSettings stateSettings = PuppetMaster.StateSettings.Default;

    [Space]
    
    [SerializeField] private int ragdollLayer;
    [SerializeField] private int pushableLayer;
    [SerializeField] private MotionController motionController;
    [SerializeField] private bool onFallActivate;
    [SerializeField] private float minGetUpVelocity = .1f;

    private Vector3 _initialControllerLocalPosition;

    public float upForce;
    public ForceMode forceMode;
    
    #region Activated

    public delegate void Activated();

    public event Activated OnActivated;

    public void InvokeActivated()
    {
        OnActivated?.Invoke();
    }

    #endregion

    #region BalanceRegained

    public delegate void BalanceRegained();

    public event BalanceRegained OnBalanceRegained;

    public void InvokeBalanceRegained()
    {
        OnBalanceRegained?.Invoke();
    }

    #endregion
    
    private void Start()
    {
        foreach (Muscle muscle in puppetMaster.muscles)
        {
            muscle.rigidbody.useGravity = false;
        }
        
        motionController.OnGroundStateChange += grounded =>
        {
            if (!grounded && onFallActivate)
            {
                Activate();
            }
        };
        
        _initialControllerLocalPosition = puppetMaster.muscles[0].transform.InverseTransformPoint(puppetMaster.targetRoot.position);
        
        behaviourPuppet.onRegainBalance.unityEvent.AddListener(delegate
        {
            InvokeBalanceRegained();
            
            puppetMaster.mode = PuppetMaster.Mode.Kinematic;

            foreach (Muscle muscle in puppetMaster.muscles)
            {
                muscle.rigidbody.interpolation = RigidbodyInterpolation.None;
                
//                muscle.rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
            }
            
            Physics.IgnoreLayerCollision(ragdollLayer, pushableLayer, true);
        });

//        behaviourPuppet.onLoseBalance.unityEvent.AddListener(Deactivate);
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
        
        else if (Keyboard.current.numpad2Key.wasPressedThisFrame)
        {
            if (puppetMaster.state != PuppetMaster.State.Dead)
            {
                Activate();

                foreach (Muscle muscle in puppetMaster.muscles)
                {
                    muscle.rigidbody.isKinematic = false;
                }
            }
            
            puppetMaster.muscles[0].rigidbody.AddForce(Vector3.up * upForce, forceMode);
        }
    }

    private void FixedUpdate()
    {
        if (puppetMaster.mode == PuppetMaster.Mode.Active)
        {
            foreach (Muscle muscle in puppetMaster.muscles)
            {
                muscle.rigidbody.AddForce(- 30f * Time.deltaTime * Vector3.up, ForceMode.VelocityChange);
            }
        }
    }

    private void LateUpdate()
    {
        if (puppetMaster.mode == PuppetMaster.Mode.Active)
        {
            Muscle hipMuscle = puppetMaster.muscles[0];
            
            if (hipMuscle.mappedVelocity.magnitude <= minGetUpVelocity)
            {
                Deactivate();
            }
        }
    }

    public void Activate()
    {
        InvokeActivated();
        
        puppetMaster.Kill(stateSettings);

        puppetMaster.targetRoot.position = puppetMaster.transform.GetChild(0).position + _initialControllerLocalPosition;
        
        foreach (Muscle muscle in puppetMaster.muscles)
        {
            muscle.rigidbody.interpolation = RigidbodyInterpolation.Interpolate;

//            muscle.rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
        
        Physics.IgnoreLayerCollision(ragdollLayer, pushableLayer, false);
    }
    
    public void Deactivate()
    {
        puppetMaster.Resurrect();
    }
}