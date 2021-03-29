using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    #region Animation

    public static readonly int RawSpeedHash = Animator.StringToHash("RawSpeed");
    public static readonly int SpeedHash = Animator.StringToHash("Speed");
    public static readonly int ForwardHash = Animator.StringToHash("Forward");
    public static readonly int RightHash = Animator.StringToHash("Right");
    public static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    public static readonly int OnAirHash = Animator.StringToHash("OnAir");
    public static readonly int OnGroundedHash = Animator.StringToHash("OnGrounded");
    public static readonly int OnStrafeMotionHash = Animator.StringToHash("OnStrafeMotion");
    public static readonly int OnFreeMotionHash = Animator.StringToHash("OnFreeMotion");
    public static readonly int VerticalDisplacementHash = Animator.StringToHash("VerticalDisplacement");

    #endregion
}
