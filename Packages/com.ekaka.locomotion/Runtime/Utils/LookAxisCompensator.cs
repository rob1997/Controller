using Locomotion.Utils.Grounder;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Locomotion.Utils
{
    //compensates for aim look in character in the y rotation axis
    //helps character look and aim correctly
    [RequireComponent(typeof(MultiAimConstraint), typeof(ExtractTransformConstraint))]
    public class LookAxisCompensator : MonoBehaviour
    {
        private MultiAimConstraint _lookConstraint;
        private ExtractTransformConstraint _extractConstraint;

        private Rig _rig;
    
        private void Awake()
        {
            _lookConstraint = GetComponent<MultiAimConstraint>();
            _extractConstraint = GetComponent<ExtractTransformConstraint>();

            if (!transform.parent.TryGetComponent(out _rig)) Debug.LogError("Axis Compensator Component needs a Rig Component on Parent Object");
        }

        private void Update()
        {
            if (_rig != null && _rig.weight > 0f)
            {
                //get spine up direction vector from spine rotation
                Vector3 spineUp = _extractConstraint.data.rotation * Vector3.up;
        
                var forward = _lookConstraint.data.sourceObjects[0].transform.position - _extractConstraint.data.position;
                //get spine forward direction vector from spine rotation
                var spineForward = _extractConstraint.data.rotation * Vector3.forward;

                float offsetY = Vector3.SignedAngle(forward, spineForward, spineUp);
                //set offset to adjust for y rotation during aim
                _lookConstraint.data.offset = new Vector3(_lookConstraint.data.offset.x, offsetY, _lookConstraint.data.offset.z);
            }
        }
    }
}
