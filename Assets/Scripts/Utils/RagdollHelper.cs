using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RagdollHelper : MonoBehaviour
{
    private struct DollPart
    {
        public readonly Rigidbody RigidBody;
        public readonly Collider Collider;
        public readonly CharacterJoint Joint;
        public readonly Vector3 ConnectedAnchorDefault;

        public DollPart(Rigidbody rBody)
        {
            RigidBody = rBody;

            Collider = rBody.GetComponent<Collider>();
            
            Joint = rBody.GetComponent<CharacterJoint>();
            
            if (Joint != null)
                ConnectedAnchorDefault = Joint.connectedAnchor;
            else
                ConnectedAnchorDefault = Vector3.zero;
        }
    }
    
    [SerializeField] private Animator animator;
    [SerializeField] private CharacterController characterController;

    private Transform _root;
    private Rigidbody _rootRBody;
    private readonly List<DollPart> _dollParts = new List<DollPart>();

    private void Start()
    {
        _root = animator.GetBoneTransform(HumanBodyBones.Hips);
        _rootRBody = _root.GetComponent<Rigidbody>();

        foreach (Rigidbody rBody in _root.GetComponentsInChildren<Rigidbody>())
        {
            _dollParts.Add(new DollPart(rBody));
        }
    }

    private void ActivateDoll(bool on)
    {
        _dollParts.ForEach(dollPart =>
        {
            
        });
    }
}
