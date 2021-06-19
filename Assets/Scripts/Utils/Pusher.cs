using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pusher : MonoBehaviour
{
    private Rigidbody _rBody;

    private void Awake()
    {
        _rBody = GetComponent<Rigidbody>();
    }

    private void Push()
    {
        
    }

    private void OnCollisionEnter(Collision other)
    {
        //Debug.LogError(other.gameObject.name);
    }
}
