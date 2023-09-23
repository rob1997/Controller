using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Sensors.Main
{
    [RequireComponent(typeof(Collider))]
    public class TargetGroup : MonoBehaviour
    {
        [SerializeField] private Target[] _targets;

        public Vector3 Center => _collider.bounds.center;
    
        private Collider _collider;

        public ITargetable Targetable { get; private set; }
        
        public void Initialize(ITargetable targetable)
        {
            Targetable = targetable;
            
            _collider = GetComponent<Collider>();
            
            foreach (Target target in _targets)
            {
                target.Initialize(this);
            }
        }
    
        public void InvokeHit(IHitData hitData)
        {
            Targetable.InvokeHit(hitData);
        }
        
        public Target GetTarget(Targeter targeter)
        {
            var targetGroups = _targets.GroupBy(t => t.Priority);

            var priorityTargets = targetGroups.OrderBy(g => Mathf.Abs(g.Key - targeter.Accuracy)).FirstOrDefault();

            if (priorityTargets != null)
            {
                return priorityTargets.OrderBy(t => Vector3.Distance(t.Center, targeter.Muzzle.position))
                    .FirstOrDefault();
            }

            return null;
        }
    }
}
