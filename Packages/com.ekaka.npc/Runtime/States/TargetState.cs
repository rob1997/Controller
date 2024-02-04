using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using NPC.Main;
using NPC.Utils;
using Sensors.Main;
using Sensors.Utils;
using UnityEngine;

public class TargetState : ContainerState<TargetState>
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

    public float TimeSinceLastSeen { get; private set; } = 0;

    private Vector3 _lastKnownPosition;

    public override void Initialize(NPCController controller)
    {
        base.Initialize(controller);

        Targeter = controller.Actor.Targeter;
    }

    protected override void EnableContainerState()
    {
        TryFindTarget();
    }

    protected override void UpdateContainerState()
    {
        if (Target != null)
        {
            LookAtTarget();
            
            CheckForTarget();
        }

        else
        {
            TimeSinceLastSeen += Time.deltaTime;
            
            TryFindTarget();
        }
    }

    private void LookAtTarget()
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
    }
    
    protected override void DisableContainerState()
    {
        Debug.Log($"Disabling {nameof(TargetState)}...");
    }

    private void TryFindTarget()
    {
        ITargetable[] results = Targeter.FindTargets<ITargetable>();
        
        results = results.Where(t => !IgnoreMask.Contains(t.Obj.tag) && LookLayerMask.HasLayer(t.Obj.layer))
            .ToArray();
        
        if (results.Length > 0)
        {
            Target = results[0].GetTarget(Targeter);
            
            TimeSinceLastSeen = 0;

            _lastKnownPosition = Target.Center;
        }
    }
    
    private void CheckForTarget()
    {
        ITargetable[] results = Targeter.FindTargets<ITargetable>();

        //break base if vision sees something
        if (results.Length > 0 && results.Contains(Target.Targetable))
        {
            TimeSinceLastSeen = 0;

            //update last known position
            _lastKnownPosition = Target.Center;
        }

        else
        {
            TimeSinceLastSeen += Time.deltaTime;
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

        eulers.x = Utils.ClampAngle(eulers.x, MinLookLimit.x + worldToLocalY,
            MaxLookLimit.x + worldToLocalY);

        float worldToLocalZ = Vector3.SignedAngle(forward, Vector3.forward, -up);

        eulers.y = Utils.ClampAngle(eulers.y, MinLookLimit.y + worldToLocalZ,
            MaxLookLimit.y + worldToLocalZ);

        return Quaternion.Euler(eulers);
    }
}