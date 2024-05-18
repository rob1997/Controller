using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using Character.Damage;
using Sensors.Main;
using Sensors.Common;
using UnityEngine;

namespace Weapon.Main
{
    [RequireComponent(typeof(Beamer))]
    public class Barrel : MonoBehaviour
    {
        [SerializeField] private Vector2[] snapshots;

        private Transform _origin;

        private BarrelGroup _group;

        [field: SerializeField] public float Power { get; private set; }
        
        public Beamer Beamer { get; private set; }

        private FirearmReference _reference;
        
        public void Initialize(Transform origin)
        {
            Beamer = GetComponent<Beamer>();

            _group = GetComponentInParent<BarrelGroup>();
            
            _reference = (FirearmReference) _group.Adapter.Item.Reference;
            
            _origin = origin;
        }

        public void Fire(int index)
        {
            index = snapshots.WrapIndex(index);

            Vector2 snapshot = snapshots[index];

            Vector3 centre = _origin.position;

            //position the Beamer units from the snapshot
            Vector3 position = centre + _origin.forward + (snapshot.x * _group.Spread * _origin.right) +
                               (snapshot.y * _group.Spread * _origin.up);

            Quaternion rotation = Quaternion.LookRotation(position - centre);

            transform.SetPositionAndRotation(position, rotation);
            
            //
            TargetHit[] hits = Beamer.FindTargets();

            if (hits == null || hits.Length == 0)
            {
                return;
            }

            float normalizedPower = 1f;
            
            //group damage pairs to avoid damage type key conflict
            var damageGroup = _reference.Damage.GroupBy(d => d.Type).ToArray();
            
            foreach (TargetHit hit in hits)
            {
                HitEffect hitEffect = _group.HitEffectSpawner.Spawn(hit.Point, Quaternion.LookRotation(hit.Normal), hit.Transform);
                
                _group.HitEffectSpawner.DeSpawn(hitEffect, _group.HitObjDestroyTimeout);
                
                Vector3 hitDirection = (hit.Point - transform.position).normalized;

                if (hit.Collider.TryGetComponent(out Rigidbody rBody))
                {
                    //multiply by mass because bullets are fast af
                    rBody.AddForceAtPosition(normalizedPower * Power * hitDirection, hit.Point);
                }
                
                if (hit.Collider.TryGetComponent(out Target target))
                {
                    if (target.Targetable is IDamageable damageable)
                    {
                        Dictionary<DamageType, float> damageSent =
                            damageGroup.ToDictionary(g => g.Key, g => normalizedPower * g.Sum(d => d.Value));

                        DamageData hitData = new DamageData(damageSent, damageable.Damager, damageable);
                    
                        if (_group.Adapter.Actor.Targeter.TryHit(target, hitData))
                        {
                            //hit
                            foreach (var pair in damageSent)
                            {
                                Debug.Log($"Hit {damageable.Obj.name} with {pair.Value} {pair.Key} damage");
                            }
                        }
                    }
                }
                
                normalizedPower -= .25f;
            
                if (normalizedPower <= 0)
                    break;
            }
        }

        public void TakeSnapshot()
        {
            snapshots = snapshots.Append(transform.localPosition).ToArray();
        }
    }
}