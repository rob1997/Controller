using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace Locomotion.Utils.Grounder
{
    [DisallowMultipleComponent]
    [AddComponentMenu("Animation Rigging/Extract Transform Constraint")]
    public class ExtractTransformConstraint : RigConstraint<ExtractTransformConstraintJob, 
        ExtractTransformConstraintData, ExtractTransformConstraintJobBinder>
    {
        
    }
}