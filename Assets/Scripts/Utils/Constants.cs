using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
    #region Animation

    public static readonly int RawSpeedHash = Animator.StringToHash("RawSpeed");
    public static readonly int SpeedHash = Animator.StringToHash("Speed");
    public static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    public static readonly int VerticalDisplacementHash = Animator.StringToHash("VerticalDisplacement");

    #endregion
}
