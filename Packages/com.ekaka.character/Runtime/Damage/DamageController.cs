using System.Collections.Generic;
using Character.Main;
using Damage.Main;
using UnityEngine;
using UnityEngine.Serialization;

namespace Character.Damage
{
    public class DamageController : Controller
    {
        [Tooltip("A velocity threshold if exceeded during falling will damage the character on landing")]
        [SerializeField] private float _fallDamageVelocityThreshold = 10f;
    
        [Tooltip("How much damage character takes based on velocity units exceeding fallDamageVelocityThreshold")]
        [SerializeField] private float _fallDamagePerUnit = 2f;
    
        public override void Initialize(Actor actor)
        {
            base.Initialize(actor);

            RegisterTargetHitDamage();
        }

        private void RegisterTargetHitDamage()
        {
            Actor.TargetHit += hitData =>
            {
                if (hitData is DamageData data)
                {
                    Actor.Damager.DealDamage(data);
                }
            };
        }
        
        public void ApplyFallDamage(float landingVelocity)
        {
            if (landingVelocity > _fallDamageVelocityThreshold)
            {
                DamageData fallDamage = new DamageData
                (new Dictionary<DamageType, float>
                {
                    {
                        DamageType.Fall,
                        (landingVelocity - _fallDamageVelocityThreshold) * _fallDamagePerUnit
                    }
                }, Actor.Damager, Actor);

                Actor.Damager.DealDamage(fallDamage);
            }
        }
    }
}
