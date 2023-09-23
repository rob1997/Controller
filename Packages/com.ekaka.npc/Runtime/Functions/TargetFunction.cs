using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using NPC.Main;
using NPC.Utils;
using Sensors.Main;
using Sensors.Utils;
using UnityEngine;

public class TargetFunction : ContainerFunction<TargetFunction>
{
    [field: SerializeField] public Transform LookFrom { get; private set; }

    [field: Space] [field: SerializeField] public float LookSpeed { get; private set; } = 25f;

    [field: Tooltip("LookSpeed is multiplied by this amount before being applied")]
    [field: SerializeField]
    public float LookSpeedMultiplier { get; private set; } = 1f;

    [field: SerializeField] public float LastSeenTimeout { get; private set; } = 2f;

    [field: Space] [field: SerializeField] public bool LimitFollowLook { get; private set; }

    [field: ShowIf(nameof(LimitFollowLook))]
    [field: SerializeField]
    public Vector2 MinLookLimit { get; private set; }

    [field: ShowIf(nameof(LimitFollowLook))]
    [field: SerializeField]
    public Vector2 MaxLookLimit { get; private set; }

    [field: Space] [field: SerializeField] public LayerMask LookLayerMask { get; private set; }

    [field: SerializeField] public TagMask IgnoreMask { get; private set; }

    public Target Target { get; private set; }
    
    public Targeter Targeter { get; private set; }

    private float _timeSinceLastSeen = 0;

    private Vector3 _lastKnownPosition;

    public override void Initialize(NPCController controller)
    {
        base.Initialize(controller);

        Targeter = controller.Actor.Targeter;
    }

    protected override void EnableContainerFunction()
    {
        ITargetable[] results = Targeter.FindTargets<ITargetable>();
        
        results = results.Where(t => !IgnoreMask.Contains(t.Obj.tag) && LookLayerMask.HasLayer(t.Obj.layer))
            .ToArray();
        
        if (results.Length > 0)
        {
            Target = results[0].GetTarget(Targeter);
            
            _timeSinceLastSeen = 0;

            _lastKnownPosition = Target.Center;
        }

        else
        {
            BreakFunction();
        }
    }

    protected override void RunContainerFunction()
    {
        Vector3 origin = LookFrom.position;

        Quaternion lookRotation = Quaternion.LookRotation(_lastKnownPosition - origin);

        //limit rotation based on limit angles
        if (LimitFollowLook)
        {
            lookRotation = LimitLookRotation(lookRotation);
        }

        //calculate duration for rotation
        Quaternion currentRotation = LookFrom.rotation;

        float delta = Quaternion.Angle(currentRotation, lookRotation);

        float duration = delta / LookSpeed;

        LookFrom.rotation = Quaternion.Lerp(currentRotation, lookRotation,
            (Time.deltaTime / duration) * LookSpeedMultiplier);

        CheckForTarget();
    }

    protected override void DisableContainerFunction()
    {
        Debug.Log($"Disabling {nameof(TargetFunction)}...");
    }

    private void CheckForTarget()
    {
        ITargetable[] results = Targeter.FindTargets<ITargetable>();

        //break base if vision sees something
        if (results.Length > 0 && results.Contains(Target.Targetable))
        {
            _timeSinceLastSeen = 0;

            //update last known position
            _lastKnownPosition = Target.Center;
        }

        else
        {
            _timeSinceLastSeen += Time.deltaTime;
        }

        //if timeout is reached disable function
        if (_timeSinceLastSeen > LastSeenTimeout)
        {
            BreakFunction();

            return;
        }
    }

    private Quaternion LimitLookRotation(Quaternion lookRotation)
    {
        //get parent directions for converting local limits to world limits
        Transform parent = LookFrom.parent;

        Vector3 up = parent != null ? parent.up : Vector3.up;

        Vector3 forward = parent != null ? parent.forward : Vector3.forward;

        Vector3 right = parent != null ? parent.right : Vector3.right;

        //get eulers and clamp angles based on limits
        Vector3 eulers = lookRotation.eulerAngles;

        float worldToLocalY = Vector3.SignedAngle(up, Vector3.up, right);

        eulers.x = Core.Utils.Utils.ClampAngle(eulers.x, MinLookLimit.x + worldToLocalY,
            MaxLookLimit.x + worldToLocalY);

        float worldToLocalZ = Vector3.SignedAngle(forward, Vector3.forward, -up);

        eulers.y = Core.Utils.Utils.ClampAngle(eulers.y, MinLookLimit.y + worldToLocalZ,
            MaxLookLimit.y + worldToLocalZ);

        return Quaternion.Euler(eulers);
    }
}