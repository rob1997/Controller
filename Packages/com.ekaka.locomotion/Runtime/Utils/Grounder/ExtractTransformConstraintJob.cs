using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace Locomotion.Utils.Grounder
{
    public struct ExtractTransformConstraintJob : IWeightedAnimationJob
    {
        public ReadWriteTransformHandle Bone;
        
        public FloatProperty jobWeight { get; set; }
   
        public Vector3Property Position;
        
        public Vector4Property Rotation;
        
        public void ProcessRootMotion(AnimationStream stream)
        { }
   
        public void ProcessAnimation(AnimationStream stream)
        {
            AnimationRuntimeUtils.PassThrough(stream, Bone);
            
            Vector3 pos = Bone.GetPosition(stream);
            Quaternion rot = Bone.GetRotation(stream);
            
            Position.Set(stream, pos);
            Rotation.Set(stream, new Vector4(rot.x, rot.y, rot.z, rot.w));
        }
    }
}
