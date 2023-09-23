using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Sensors.Main
{
    //custom hit wrapper for ray and collider cast hits
    public struct TargetHit
    {
        public Collider Collider { get; private set; }
        
        //hit point
        public Vector3 Point { get; private set; }
        
        public Vector3 Normal { get; private set; }

        public Transform Transform => Collider.transform;
        
        public string Tag => Collider.tag;
        
        public int Layer => Collider.gameObject.layer;

        public TargetHit(Collider collider, Transform origin = null)
        {
            Collider = collider;

            Point = origin != null ? collider.ClosestPoint(origin.position) : collider.bounds.center;

            Normal = origin != null ? - (Point - origin.position).normalized : Point;
        }

        public TargetHit(RaycastHit hit)
        {
            Collider = hit.collider;
            
            Point = hit.point;

            Normal = hit.normal;
        }

        public T GetComponent<T>()
        {
            return Transform.GetComponent<T>();
        }
    }
    
    public abstract class Targeter : MonoBehaviour
    {
        [field: SerializeField] public float Range { get; private set; } = 5f;
        
        [Tooltip("how many simultaneous hits/targets")]
        [field: SerializeField] public int Size { get; private set; } = 5;
        
        [field: Space]
        
        [field: SerializeField] public Vector2 ErrorMargin { get; private set; }

        [field: Range(0f, 1f)]
        [field: SerializeField] public float Accuracy { get; private set; } = 1f;
        
        //targeting origin
        [field: SerializeField] public Transform Muzzle { get; private set; }

        [field: Space]
        
        [field: SerializeField] public LayerMask TargetLayerMask { get; private set; }

        [field: SerializeField] public TagMask IgnoreTagMask { get; private set; }

        protected TargetHit[] Hits { get; private set; }
        
        private bool _hitsCached;
        
        public bool TryHit(Target target, IHitData hitData)
        {
            Vector2 randomPoint = Random.insideUnitCircle;

            Transform targetTransform = target.transform;
            
            Vector3 hitPoint = target.Center + (randomPoint.x * ErrorMargin.x * targetTransform.right) +
                               (randomPoint.y * ErrorMargin.y * targetTransform.up);

            return target.TryHit(hitPoint, hitData);
        }

        public TargetHit[] FindTargets()
        {
            if (_hitsCached)
            {
                return Hits;
            }

            Hits = CastView();

            _hitsCached = true;
            
            return Hits;
        }

        //cast view to find targets and return targets hit
        protected abstract TargetHit[] CastView();
        
        public T[] FindTargets<T>() where T : ITargetable
        {
            TargetHit[] hits = FindTargets();

            T[] components = Array.ConvertAll(hits, c => c.GetComponent<T>());

            return components.Where(c => c != null).ToArray();
        }
        
        private void LateUpdate()
        {
            _hitsCached = false;
        }
    }
}
