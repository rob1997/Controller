using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.Dynamics
{

    public class PuppetMasterLite : MonoBehaviour
    {
        public Transform targetRoot;
        public bool fixTargetTransforms = true;
        public float blendTime = 0.1f;
        [Range(0f, 1f)] public float mappingWeight = 1f;
        [Range(0f, 1f)] public float pinWeight = 1f;
        [Range(0f, 1f)] public float muscleWeight = 1f;
        
        public float muscleSpring = 1000f;
        public float muscleDamper = 100f;
        public bool updateJointAnchors = true;
        public bool angularPinning;

        [LargeHeader("Individual Muscle Settings")]
        public MuscleLite[] muscles = new MuscleLite[0];

        public delegate void PuppetMasterLiteDelegate();
        public PuppetMasterLiteDelegate OnRead;
        public PuppetMasterLiteDelegate OnWrite;

        private Animator targetAnimator;
        private bool animatorDisabled;
        private bool fixedFrame;
        private UpdateMode updateMode = UpdateMode.Normal;

        public enum UpdateMode
        {
            Normal,
            Fixed
        }

        private void Start()
        {
            Initiate();
        }

        public void Activate()
        {
            if (gameObject.activeInHierarchy) return;

            mappingWeight = 0f;
            foreach (MuscleLite m in muscles)
            {
                m.Reset();
            }
            
            gameObject.SetActive(true);

            foreach (MuscleLite m in muscles)
            {
                m.rigidbody.WakeUp();
                m.MoveToTarget();
                m.ClearVelocities();
            }

            Read();

            //Debug.Break();
            //return;
            
            StopAllCoroutines();
            StartCoroutine(Activation());
        }

        private IEnumerator Activation()
        {
            if (blendTime <= 0f)
            {
                mappingWeight = 1f;
                yield break;
            }

            while (mappingWeight < 1f)
            {
                mappingWeight = Mathf.MoveTowards(mappingWeight, 1f, Time.deltaTime * (1f / blendTime));
                yield return null;
            }
        }

        public void Deactivate()
        {
            if (!gameObject.activeInHierarchy) return;

            StopAllCoroutines();
            StartCoroutine(Deactivation());
        }

        private IEnumerator Deactivation()
        {
            if (blendTime > 0f)
            { 
                while (mappingWeight > 0f)
                {
                    mappingWeight = Mathf.MoveTowards(mappingWeight, 0f, Time.deltaTime * (1f / blendTime));
                    yield return null;
                }
            }

            if (animatorDisabled) targetAnimator.enabled = true;
            animatorDisabled = false;
            gameObject.SetActive(false);
        }

        private void Initiate()
        {
            if (targetRoot.gameObject.layer == gameObject.layer) Debug.LogError("Target Root is on the same layer as PuppetMasterLite! Please use different layers and make sure collisions between those layers are disabled in the Layer Collision Matrix.", transform);

            targetAnimator = targetRoot.GetComponentInChildren<Animator>();
            if (targetAnimator != null && targetAnimator.updateMode == AnimatorUpdateMode.AnimatePhysics) updateMode = UpdateMode.Fixed;

            foreach (MuscleLite m in muscles)
            {
                m.Initiate(muscles);
                m.mappingWeightMlp = 0f;
            }
        }

        private void Update()
        {
            updateMode = targetAnimator == null || targetAnimator.updateMode != AnimatorUpdateMode.AnimatePhysics ? UpdateMode.Normal : UpdateMode.Fixed;
            if (updateMode == UpdateMode.Fixed) return;

            FixTargetTransforms();
        }

        private void FixTargetTransforms()
        {
            if (!fixTargetTransforms) return;

            foreach (MuscleLite m in muscles)
            {
                m.FixTargetTransforms();
            }
        }

        private void FixedUpdate()
        {
            fixedFrame = true;

            if (updateMode == UpdateMode.Fixed)
            {
                FixTargetTransforms();

                if (targetAnimator.enabled || (!targetAnimator.enabled && animatorDisabled))
                {
                    targetAnimator.enabled = false;
                    animatorDisabled = true;
                    targetAnimator.Update(Time.fixedDeltaTime);
                }
                else
                {
                    animatorDisabled = false;
                    targetAnimator.enabled = false;
                }

                Read();
            }

            foreach (MuscleLite m in muscles)
            {
                m.Update(pinWeight, muscleWeight, muscleSpring, muscleDamper, angularPinning);
            }
        }
       
        private void LateUpdate()
        {
            if (animatorDisabled) targetAnimator.enabled = true;
            animatorDisabled = false;

            switch (updateMode)
            {
                case UpdateMode.Fixed:
                    if (fixedFrame) Write();
                    break;
                default:
                    Read();
                    Write();
                    break;
            }

            fixedFrame = false;
        }

        private void Read()
        {
            if (OnRead != null) OnRead();

            foreach (MuscleLite m in muscles)
            {
                m.Read();
            }

            if (updateJointAnchors)
            {
                foreach (MuscleLite m in muscles)
                {
                    m.UpdateAnchor(true);
                }
            }
        }

        private void Write()
        {
            foreach (MuscleLite m in muscles)
            {
                m.Map(mappingWeight);
            }

            if (OnWrite != null) OnWrite();
        }

        private void OnDrawGizmosSelected()
        {
            if (Application.isPlaying) return;

            for (int i = 0; i < muscles.Length; i++)
            {
                muscles[i].name = i.ToString() + ": " + (muscles[i].joint != null ? muscles[i].joint.name : "Missing Joint reference!");
            }
        }
    }
}
