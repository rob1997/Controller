using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Playground : MonoBehaviour
{
    private CharacterController _characterController;
    private Animator _animator;

    private Transform _root;
    
    // Start is called before the first frame update
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();

        _root = _animator.GetBoneTransform(HumanBodyBones.Hips);
    }

    // Update is called once per frame
    void Update()
    {
        Debug.LogError(_root.position);
    }
}
