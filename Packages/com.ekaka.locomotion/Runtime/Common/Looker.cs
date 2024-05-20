using System;
using System.Collections;
using System.Collections.Generic;
using Locomotion.Main;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Locomotion.Common
{
    [RequireComponent(typeof(MotionController))]
    public class Looker : MonoBehaviour
    {
        [SerializeField] private Rig _lookRig;

        private MotionController _motionController;
    
        private void Awake()
        {
            _motionController = GetComponent<MotionController>();
        }

        private void Start()
        {
            if (_motionController.IsReady) RegisterLook();
        
            else _motionController.OnReady += RegisterLook;
        }

        private void RegisterLook()
        {
            _motionController.OnLookModeChange += mode =>
            {
                switch (mode)
                {
                    case MotionController.LookMode.Free:
                        _lookRig.weight = 0;
                        break;
                    case MotionController.LookMode.Strafe:
                        _lookRig.weight = 1;
                        break;
                }
            };
        }
    }
}
