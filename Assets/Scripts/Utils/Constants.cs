using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Constants
{
    public static class Animation
    {
        public static readonly int RawSpeedHash = Animator.StringToHash("RawSpeed");
        
        public static readonly int SpeedHash = Animator.StringToHash("Speed");
        public static readonly int ForwardHash = Animator.StringToHash("Forward");
        public static readonly int RightHash = Animator.StringToHash("Right");
        public static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
        public static readonly int OnAirHash = Animator.StringToHash("OnAir");
        public static readonly int OnLandHash = Animator.StringToHash("OnLand");
        public static readonly int OnStrafeMotionHash = Animator.StringToHash("OnStrafeMotion");
        public static readonly int OnFreeMotionHash = Animator.StringToHash("OnFreeMotion");
        public static readonly int NormalizedVerticalDisplacementHash = Animator.StringToHash("NormalizedVerticalDisplacement");
        public static readonly int VerticalDistanceHash = Animator.StringToHash("VerticalDistance");

        public static readonly string IdleStateShortName = "Idle";
        public static readonly string MotionStateShortName = "MotionTree";
        public static readonly string AirborneStateShortName = "Airborne";
        public static readonly string EmptyStateShortName = "Empty";
        public static readonly string LandStateShortName = "Land";
    
        public static readonly Dictionary<string, int> StateHashes = new Dictionary<string, int>
        {
            {IdleStateShortName, Animator.StringToHash(IdleStateShortName)},
            {MotionStateShortName, Animator.StringToHash(MotionStateShortName)},
            {AirborneStateShortName, Animator.StringToHash(AirborneStateShortName)},
            {EmptyStateShortName, Animator.StringToHash(EmptyStateShortName)},
            {LandStateShortName, Animator.StringToHash(LandStateShortName)},
        };

        public static bool HasState(int hash)
        {
            return StateHashes.ContainsValue(hash);
        }
        
        public static string GetState(int hash)
        {
            return StateHashes.First(h => h.Value == hash).Key;
        }
    }
}
