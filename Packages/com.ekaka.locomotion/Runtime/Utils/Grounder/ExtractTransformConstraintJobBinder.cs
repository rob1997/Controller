using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Locomotion.Utils.Grounder
{
    public class ExtractTransformConstraintJobBinder : AnimationJobBinder<ExtractTransformConstraintJob, ExtractTransformConstraintData>
    {
        public override ExtractTransformConstraintJob Create(Animator animator,
            ref ExtractTransformConstraintData data, Component component)
        {
            return new ExtractTransformConstraintJob
            {
                Bone = ReadWriteTransformHandle.Bind(animator, data.bone),
            
                Position = Vector3Property.Bind(animator, component, "m_Data." + nameof(data.position)),
                
                Rotation = Vector4Property.Bind(animator, component, "m_Data." + nameof(data.rotation)),
            };
        }

        public override void Destroy(ExtractTransformConstraintJob job)
        {
        
        }
    }
}
