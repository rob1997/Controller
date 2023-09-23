using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Locomotion.Utils.Grounder
{
    [Serializable]
    public struct ExtractTransformConstraintData : IAnimationJobData
    {
        [SyncSceneToStream] public Transform bone;
        
        [HideInInspector] public Vector3 position;
        
        [HideInInspector] public Quaternion rotation;
        
        public bool IsValid()
        {
            return bone != null;
        }
 
        public void SetDefaultValues()
        {
            bone = null;
            
            position = Vector3.zero;
            
            rotation = Quaternion.identity;
        }
    }
}