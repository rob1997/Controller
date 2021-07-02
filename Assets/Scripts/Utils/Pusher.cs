using System;
using System.Collections;
using System.Collections.Generic;
using RootMotion.Dynamics;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class Pusher : MonoBehaviour
{
    private Rigidbody _rBody;
    private Collider _collider;

    public Ragdoll ragdoll;

    public float force;

    private bool _applyForce;
    
    private void Awake()
    {
        _rBody = GetComponent<Rigidbody>();
        _collider = GetComponent<Collider>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.TryGetComponent(out Character character) && ragdoll.puppetMaster.state != PuppetMaster.State.Dead)
        {
            ragdoll.Activate();

            _rBody.isKinematic = true;
            
            ragdoll.puppetMaster.muscles[0].rigidbody.AddForce(Vector3.up * force, ForceMode.VelocityChange);
        }
    }

    private void Update()
    {
        if (Keyboard.current.numpad3Key.wasPressedThisFrame)
        {
            ragdoll.Activate();
            
            _rBody.isKinematic = true;

            _applyForce = true;
        }

        if (ragdoll.puppetMaster.mode == PuppetMaster.Mode.Active && _applyForce)
        {
            ragdoll.puppetMaster.muscles[0].rigidbody.AddForce(Vector3.up * force, ForceMode.VelocityChange);

            _applyForce = false;
        }
    }
}
