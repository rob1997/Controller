using System;
using System.Collections;
using System.Collections.Generic;
using Locomotion.Controllers;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Locomotion.Utils.Grounder
{
    public class Grounder : MonoBehaviour
    {
        [SerializeField] public bool active = true;
        
        [Space]
        
        [Header("IK")]
        [SerializeField] private Rig ikRig;
        
        [Space]
        
        [SerializeField] private TwoBoneIKConstraint ikConstraintL;
        [SerializeField] private TwoBoneIKConstraint ikConstraintR;
        
        [Space]
        
        [SerializeField] private ExtractTransformConstraint extractConstraintPelvis;
        
        [Space]
        
        [SerializeField] private ExtractTransformConstraint extractConstraintL;
        [SerializeField] private ExtractTransformConstraint extractConstraintR;
        
        [Space]
        [Header("Transforms")]
        
        [Tooltip("Not Actual Pelvis Bone, Substitute in Pelvis MultiPosition Constraint (Source Object 0)")]
        [SerializeField] private Transform pelvis;
        
        [Space]
        
        [Tooltip("Transform Facing Forward of Left Foot")]
        [SerializeField] private Transform footForwardL;
        [Tooltip("Transform Facing Forward of Left Foot")]
        [SerializeField] private Transform footForwardR;
        
        [Space]
        
        [Header("Presets")]
        
        [SerializeField] private float ankleHeight = .1f;
        
        [Space] 
        
        //limits for adjusting pelvis
        [SerializeField] private float minStepHeight = .05f;
        [SerializeField] public float maxStepHeight = .5f;
        
        [Space] 
        
        //blend/lerp speed
        [SerializeField] private float pelvisMoveSpeed;
        [SerializeField] private float feetIkSpeed;

        [HideInInspector] public float ikRotationWeightL;
        [HideInInspector] public float ikRotationWeightR;
        
        //transform created as child of footForward to adjust for relative rotation 
        private Transform _footPlacementL;
        private Transform _footPlacementR;

        //for blending/lerping since values get reset every frame in animation cycle
        private Vector3 _lastPelvisPosition;

        //just the y component
        private float _lastIkPositionL;
        private float _lastIkPositionR;

        private Quaternion _lastIkRotationL;
        private Quaternion _lastIkRotationR;

        private void Start()
        {
            //Create Child Transforms for relative Rotation IK and assign to initial foot rotation to get rotation offset
            GameObject footPlacementObjL = new GameObject("FootPlacementL");
            _footPlacementL = footPlacementObjL.transform;
            _footPlacementL.SetParent(footForwardL);
            _footPlacementL.localPosition = Vector3.zero;
            _footPlacementL.rotation = ikConstraintL.data.tip.rotation;

            GameObject footPlacementObjR = new GameObject("FootPlacementR");
            _footPlacementR = footPlacementObjR.transform;
            _footPlacementR.SetParent(footForwardR);
            _footPlacementR.localPosition = Vector3.zero;
            _footPlacementR.rotation = ikConstraintR.data.tip.rotation;
        }

        private void Update()
        {
            //Get all original Bone Positions
            Vector3 pelvisPosition = extractConstraintPelvis.data.position;

            Vector3 bonePositionL = extractConstraintL.data.position;
            Vector3 bonePositionR = extractConstraintR.data.position;

            Quaternion boneRotationL = extractConstraintL.data.rotation;
            Quaternion boneRotationR = extractConstraintR.data.rotation;

            ikRig.weight = active ? 1f : 0f;

            if (!active)
            {
                _lastPelvisPosition = pelvisPosition;

                _lastIkPositionL = bonePositionL.y;
                _lastIkPositionR = bonePositionR.y;

                _lastIkRotationL = boneRotationL;
                _lastIkRotationR = boneRotationR;
                
                return;
            }
            
            //left Foot Raycast
            Vector3 originL = bonePositionL;

            originL.y += maxStepHeight;

            bool leftHit = Physics.Raycast(originL, Vector3.down, out RaycastHit hitL, maxStepHeight * 2f);

            //right Foot Raycast
            Vector3 originR = bonePositionR;

            originR.y += maxStepHeight;

            bool rightHit = Physics.Raycast(originR, Vector3.down, out RaycastHit hitR, maxStepHeight * 2f);

            bool hit = leftHit && rightHit;

            //displacement between legs
            float delta = hitL.point.y - hitR.point.y;

            //distance between legs
            float offset = Mathf.Abs(delta);

            bool adjustPelvis = offset <= maxStepHeight && offset >= minStepHeight && hit;

            if (adjustPelvis)
            {
                //move pelvis down (always down)
                pelvisPosition.y -= offset;

                //re-adjust right foot for pelvis movement
                if (delta < 0)
                {
                    bonePositionR.y = hitR.point.y + ankleHeight;

                    //rotation R
                    boneRotationR = SolveRotation(hitR.normal, footForwardR, ref _footPlacementR);
                }

                else if (delta > 0)
                {
                    bonePositionL.y = hitL.point.y + ankleHeight;

                    //rotation L
                    boneRotationL = SolveRotation(hitL.normal, footForwardL, ref _footPlacementL);
                }
            }

            //Apply lerped pelvis readjustment, foot ik position and rotation
            
            //pelvis
            AdjustPelvis(pelvisPosition);

            //IK
            float t = feetIkSpeed * Time.deltaTime;

            //ik position
            ApplyIkPosition(ref _lastIkPositionL, ref ikConstraintL, bonePositionL, t);
            ApplyIkPosition(ref _lastIkPositionR, ref ikConstraintR, bonePositionR, t);

            //ik rotation
            SetIkRotationWeight();
            ApplyIkRotation(ref _lastIkRotationL, ref ikConstraintL, boneRotationL, t);
            ApplyIkRotation(ref _lastIkRotationR, ref ikConstraintR, boneRotationR, t);
        }

        private void AdjustPelvis(Vector3 pelvisPosition)
        {
            _lastPelvisPosition = Vector3.Lerp(_lastPelvisPosition, pelvisPosition, pelvisMoveSpeed * Time.deltaTime);

            pelvis.position = _lastPelvisPosition;
        }
        
        private void ApplyIkPosition(ref float lastIkPosition, ref TwoBoneIKConstraint ikConstraint, Vector3 bonePosition, float t)
        {
            //ik position R
            lastIkPosition = Mathf.Lerp(lastIkPosition, bonePosition.y, t);

            bonePosition.y = lastIkPosition;
            
            ikConstraint.data.target.position = bonePosition;
        }
        
        private void ApplyIkRotation(ref Quaternion lastIkRotation, ref TwoBoneIKConstraint ikConstraint, Quaternion boneRotation, float t)
        {
            lastIkRotation = Quaternion.Lerp(lastIkRotation, boneRotation, t);

            ikConstraint.data.target.rotation = lastIkRotation;
        }
        
        private void SetIkRotationWeight()
        {
            ikConstraintL.data.targetRotationWeight = ikRotationWeightL;
            ikConstraintR.data.targetRotationWeight = ikRotationWeightR;
        }
        
        private Quaternion SolveRotation(Vector3 normal, Transform footForward, ref Transform footPlacement)
        {
            Vector3 localNormal = transform.InverseTransformDirection(normal);

            footForward.localRotation = Quaternion.FromToRotation(Vector3.up, localNormal);

            return footPlacement.rotation;
        }
    }
}