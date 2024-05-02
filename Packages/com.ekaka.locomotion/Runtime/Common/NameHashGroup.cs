using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Locomotion.Common
{
    public static class NameHashGroup
    {
        public static float WalkBlendValue = 1f;
        public static float RunBlendValue = 2f;
        public static float SprintBlendValue = 3f;
        
        public static float DampTime = .15f;
        
        private struct NameHash
        {
            public string Name { get; private set; }
            public int Hash { get; private set; }

            public NameHash(string name)
            {
                Name = name;
                Hash = Animator.StringToHash(Name);
            }
        }
        
        public static readonly int ForwardHash = Animator.StringToHash("Forward");
        public static readonly int RightHash = Animator.StringToHash("Right");
        public static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        public static readonly int OnAirHash = Animator.StringToHash("OnAir");
        public static readonly int OnLandHash = Animator.StringToHash("OnLand");
        public static readonly int OnStrafeMotionHash = Animator.StringToHash("OnStrafeMotion");
        public static readonly int OnFreeMotionHash = Animator.StringToHash("OnFreeMotion");
        public static readonly int NormalizedVerticalDisplacementHash = Animator.StringToHash("NormalizedVerticalDisplacement");
        public static readonly int VerticalDistanceHash = Animator.StringToHash("VerticalDistance");
        
        public static readonly int LeftFootUpHash = Animator.StringToHash("LeftFootUp");
        public static readonly int RightFootUpHash = Animator.StringToHash("RightFootUp");

        private static readonly NameHash[] TrackedNameHashes =
        {
            new NameHash("Idle"), 
            new NameHash("MotionTree"), 
            new NameHash("Airborne"), 
            new NameHash("Empty"), 
            new NameHash("Land"), 
        };

        private static bool HasState(int hash)
        {
            return TrackedNameHashes.Any(s => s.Hash == hash);
        }

        public static bool IsTracked(this AnimatorStateInfo stateInfo)
        {
            return HasState(stateInfo.shortNameHash);
        }
    }
}
