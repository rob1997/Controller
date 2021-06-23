using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.Dynamics
{
    [System.Serializable]
    public class MuscleLite
    {
        [HideInInspector] public string name;
        public ConfigurableJoint joint;
        public Transform target;
        public float pinWeightMlp = 1f;
        public float muscleWeightMlp = 1f;
        public float muscleDamperMlp = 1f;
        public float mappingWeightMlp = 1f;
        
        public Transform transform { get; private set; }
        public Rigidbody rigidbody { get; private set; }
        public Vector3 positionOffset { get; private set; }
        public int index { get; private set; }
        
        private JointDrive slerpDrive = new JointDrive();
        private Quaternion defaultLocalRotation = Quaternion.identity;
        private Quaternion toJointSpaceInverse = Quaternion.identity;
        private Quaternion toJointSpaceDefault = Quaternion.identity;
        private Quaternion targetAnimatedRotation = Quaternion.identity;
        private Quaternion defaultTargetLocalRotation = Quaternion.identity;
        private Quaternion toParentSpace = Quaternion.identity;
        private Quaternion localRotationConvert = Quaternion.identity;
        private Quaternion targetAnimatedWorldRotation = Quaternion.identity;
        private Quaternion defaultRotation = Quaternion.identity;
        private Vector3 defaultPosition;
        private Vector3 defaultTargetLocalPosition;
        private float lastJointDriveRotationWeight, lastRotationDamper;
        private bool initiated;
        private Transform connectedBodyTarget;
        private Transform connectedBodyTransform;
        private Transform targetParent;
        private bool directTargetParent;
        private Vector3 targetVelocity;
        private Vector3 targetAnimatedCenterOfMass;
        
        public void Initiate(MuscleLite[] colleagues)
        {
            name = joint.name;
            transform = joint.transform;
            rigidbody = joint.GetComponent<Rigidbody>();

            if (joint.connectedBody != null)
            {
                for (int i = 0; i < colleagues.Length; i++)
                {
                    if (colleagues[i].joint.GetComponent<Rigidbody>() == joint.connectedBody)
                    {
                        connectedBodyTarget = colleagues[i].target;
                    }
                    if (colleagues[i] == this) index = i;
                }

                joint.autoConfigureConnectedAnchor = false;
                connectedBodyTransform = joint.connectedBody.transform;

                directTargetParent = target.parent == connectedBodyTarget;
            }

            targetParent = connectedBodyTarget != null ? connectedBodyTarget : target.parent;
            toParentSpace = Quaternion.Inverse(targetParentRotation) * parentRotation;
            localRotationConvert = Quaternion.Inverse(targetLocalRotation) * localRotation;

            // Joint space
            Vector3 forward = Vector3.Cross(joint.axis, joint.secondaryAxis).normalized;
            Vector3 up = Vector3.Cross(forward, joint.axis).normalized;

            defaultLocalRotation = localRotation;
            Quaternion toJointSpace = Quaternion.LookRotation(forward, up);
            toJointSpaceInverse = Quaternion.Inverse(toJointSpace);
            toJointSpaceDefault = defaultLocalRotation * toJointSpace;

            // Set joint params
            joint.rotationDriveMode = RotationDriveMode.Slerp;
            joint.configuredInWorldSpace = false;

            // Fix target Transforms
            defaultTargetLocalPosition = target.localPosition;
            defaultTargetLocalRotation = target.localRotation;
            targetAnimatedCenterOfMass = V3Tools.TransformPointUnscaled(target, rigidbody.centerOfMass);

            // Resetting
            if (joint.connectedBody == null)
            {
                defaultPosition = transform.localPosition;
                defaultRotation = transform.localRotation;
            }
            else
            {
                defaultPosition = joint.connectedBody.transform.InverseTransformPoint(transform.position);
                defaultRotation = Quaternion.Inverse(joint.connectedBody.transform.rotation) * transform.rotation;
            }

            rigidbody.isKinematic = false;

            Read();

            initiated = true;
        }

        public void FixTargetTransforms()
        {
            if (!initiated) return;

            target.localRotation = defaultTargetLocalRotation;
            target.localPosition = defaultTargetLocalPosition;
        }

        // Reset the Transform to the default state. This is necessary for activating/deactivating the ragdoll without messing it up
        public void Reset()
        {
            if (!initiated) return;
            if (joint == null) return;
            
            if (joint.connectedBody == null)
            {
                transform.localPosition = defaultPosition;
                transform.localRotation = defaultRotation;
            }
            else
            {
                transform.position = joint.connectedBody.transform.TransformPoint(defaultPosition);
                transform.rotation = joint.connectedBody.transform.rotation * defaultRotation;
            }

            lastRotationDamper = -1f;
        }

        // Moves and rotates the muscle to match it's target
        public void MoveToTarget()
        {
            if (!initiated) return;
            
            // Moving rigidbodies only won't animate the pose. MoveRotation does not work on a kinematic Rigidbody that is connected to another by a Joint
            transform.position = target.position;
            transform.rotation = target.rotation;
            rigidbody.MovePosition(transform.position);
            rigidbody.MoveRotation(transform.rotation);
        }

        public void ClearVelocities()
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.angularVelocity = Vector3.zero;

            targetVelocity = Vector3.zero;
            targetAnimatedCenterOfMass = V3Tools.TransformPointUnscaled(target, rigidbody.centerOfMass);
        }

        public void Read()
        {
            Vector3 tAM = V3Tools.TransformPointUnscaled(target, rigidbody.centerOfMass);
            targetVelocity = (tAM - targetAnimatedCenterOfMass) / Time.deltaTime;
            targetAnimatedCenterOfMass = tAM;

            if (joint.connectedBody != null)
            {
                targetAnimatedRotation = targetLocalRotation * localRotationConvert;
            }

            targetAnimatedWorldRotation = target.rotation;
        }

        public void Update(float pinWeightMaster, float muscleWeightMaster, float muscleSpring, float muscleDamper, bool angularPinning)
        {
            Pin(pinWeightMaster, 4, 0f, angularPinning);
            MuscleRotation(muscleWeightMaster, muscleSpring, muscleDamper);
        }

        private void Pin(float pinWeightMaster, float pinPow, float pinDistanceFalloff, bool angularPinning)
        {
            positionOffset = targetAnimatedCenterOfMass - rigidbody.worldCenterOfMass;
            if (float.IsNaN(positionOffset.x)) positionOffset = Vector3.zero;

            float w = pinWeightMaster * pinWeightMlp;

            if (w <= 0f) return;
            w = Mathf.Pow(w, pinPow);

            if (Time.deltaTime > 0f) positionOffset /= Time.deltaTime;
            Vector3 force = -rigidbody.velocity + targetVelocity + positionOffset;
            force *= w;
            if (pinDistanceFalloff > 0f) force /= 1f + positionOffset.sqrMagnitude * pinDistanceFalloff;

            rigidbody.AddForce(force, ForceMode.VelocityChange);

            // Angular pinning
            if (angularPinning)
            {
                Vector3 torque = PhysXTools.GetAngularAcceleration(rigidbody.rotation, targetAnimatedWorldRotation);

                torque -= rigidbody.angularVelocity;
                torque *= w;
                rigidbody.AddTorque(torque, ForceMode.VelocityChange);
            }
        }

        private void MuscleRotation(float muscleWeightMaster, float muscleSpring, float muscleDamper)
        {
            float w = muscleWeightMaster * muscleSpring * muscleWeightMlp * 10f;

            if (joint.connectedBody == null) w = 0f;
            else if (w > 0f) joint.targetRotation = LocalToJointSpace(targetAnimatedRotation);

            float d = muscleDamper * muscleDamperMlp;

            if (w == lastJointDriveRotationWeight && d == lastRotationDamper) return;
            lastJointDriveRotationWeight = w;

            lastRotationDamper = d;
            slerpDrive.positionSpring = w;
            slerpDrive.maximumForce = Mathf.Max(w, d);
            slerpDrive.positionDamper = d;

            joint.slerpDrive = slerpDrive;
        }

        public void Map(float masterWeight)
        {
            float w = masterWeight * mappingWeightMlp;
            if (w <= 0f) return;

            if (w >= 1f)
            {
                // Rotation
                target.rotation = transform.rotation;

                // Position
                if (connectedBodyTransform != null)
                {
                    Vector3 relativePosition = connectedBodyTransform.InverseTransformPoint(transform.position);
                    target.position = connectedBodyTarget.TransformPoint(relativePosition);
                }
                else
                {
                    target.position = transform.position;
                }

                return;
            }

            // Rotation
            target.rotation = Quaternion.Lerp(target.rotation, transform.rotation, w);

            // Position
            if (connectedBodyTransform != null)
            {
                Vector3 relativePosition = connectedBodyTransform.InverseTransformPoint(transform.position);
                target.position = Vector3.Lerp(target.position, connectedBodyTarget.TransformPoint(relativePosition), w);
            }
            else
            {
                target.position = Vector3.Lerp(target.position, transform.position, w);
            }
        }

        // Update Joint connected anchor
        public void UpdateAnchor(bool supportTranslationAnimation)
        {
            //if (state.isDisconnected) return;
            if (joint.connectedBody == null || connectedBodyTarget == null) return;
            if (directTargetParent && !supportTranslationAnimation) return;
            
            Vector3 anchorUnscaled = joint.connectedAnchor = InverseTransformPointUnscaled(connectedBodyTarget.position, connectedBodyTarget.rotation * toParentSpace, target.position);
            float uniformScaleF = 1f / connectedBodyTransform.lossyScale.x;

            joint.connectedAnchor = anchorUnscaled * uniformScaleF;
        }

        private Quaternion localRotation
        {
            get
            {
                return Quaternion.Inverse(parentRotation) * transform.rotation;
            }
        }

        // Get the rotation of the target
        private Quaternion targetLocalRotation
        {
            get
            {
                return Quaternion.Inverse(targetParentRotation * toParentSpace) * target.rotation;
            }
        }

        private Quaternion parentRotation
        {
            get
            {
                if (joint.connectedBody != null) return joint.connectedBody.rotation;
                if (transform.parent == null) return Quaternion.identity;
                return transform.parent.rotation;
            }
        }

        private Quaternion targetParentRotation
        {
            get
            {
                if (targetParent == null) return Quaternion.identity;
                return targetParent.rotation;
            }
        }

        // Convert a local rotation to local joint space rotation
        private Quaternion LocalToJointSpace(Quaternion localRotation)
        {
            return toJointSpaceInverse * Quaternion.Inverse(localRotation) * toJointSpaceDefault;
        }

        // Inversetransforms a point by the specified position and rotation
        private static Vector3 InverseTransformPointUnscaled(Vector3 position, Quaternion rotation, Vector3 point)
        {
            return Quaternion.Inverse(rotation) * (point - position);
        }
    }
}
