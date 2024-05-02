using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Locomotion.Common
{
    [RequireComponent(typeof(CharacterController))]
    public class BasicRigidBodyPush : MonoBehaviour
    {
        public LayerMask pushLayers;
        public bool canPush;
        public bool interpolate;
        public float strength = 1f;

        private Rigidbody _body;

        private const float _interpolationRevertTime = 1f;
    
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (canPush) PushRigidBodies(hit);
        }

        private void PushRigidBodies(ControllerColliderHit hit)
        {
            // make sure we hit a non kinematic rigidbody
            _body = hit.collider.attachedRigidbody;
        
            if (_body == null || _body.isKinematic) return;

            // make sure we only push desired layer(s)
            var bodyLayerMask = 1 << _body.gameObject.layer;
            if ((bodyLayerMask & pushLayers.value) == 0) return;

            // We dont want to push objects below us
            if (hit.moveDirection.y < -0.3f) return;

            if (interpolate && _body.interpolation != RigidbodyInterpolation.Interpolate)
            {
                _body.interpolation = RigidbodyInterpolation.Interpolate;

                StartCoroutine(RevertInterpolation());
            }
        
            // Calculate push direction from move direction, horizontal motion only
            Vector3 pushDir = new Vector3(hit.moveDirection.x, 0.0f, hit.moveDirection.z);

            // Apply the push and take strength into account
            _body.AddForceAtPosition(pushDir * strength, hit.point, ForceMode.Impulse);
        }

        IEnumerator RevertInterpolation()
        {
            Rigidbody body = _body;
        
            yield return new WaitForSeconds(_interpolationRevertTime);

            if (_body != body && body.interpolation != RigidbodyInterpolation.None)
            {
                body.interpolation = RigidbodyInterpolation.None;
            }
        }
    }
}