using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sensors.Main
{
    [RequireComponent(typeof(Collider))]
    public class Target : MonoBehaviour
    {
        private Collider _collider;

        [field: Range(0f, 1f)]
        [Tooltip("on a normalized scale which target to prioritize when targeting a group ")]
        [field: SerializeField] public float Priority { get; private set; }

        public Vector3 Center => _collider.bounds.center;
        
        public TargetGroup ContainerTargetGroup { get; private set; }

        public ITargetable Targetable => ContainerTargetGroup.Targetable;

        public void Initialize(TargetGroup targetGroup)
        {
            _collider = GetComponent<Collider>();
            
            ContainerTargetGroup = targetGroup;
        }

        public bool TryHit(Vector3 hitPoint, IHitData hitData)
        {
            if (_collider.bounds.Contains(hitPoint))
            {
                ContainerTargetGroup.InvokeHit(hitData);
                
                return true;
            }

            return false;
        }
    }
}
