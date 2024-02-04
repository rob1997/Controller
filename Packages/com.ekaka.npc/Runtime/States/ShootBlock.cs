using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Damage.Main;
using NPC.Main;
using Sensors.Main;

namespace NPC.States
{
    public class ShootBlock : StateBlock<TargetState>
    {
        [Tooltip("How many shots per second")]
        [SerializeField] private float _fireRate;

        [SerializeField] private float _damage;

        private float _time;

        private Targeter Targeter => ContainerState.Targeter;

        private IDamagable _damagable;

        private IDamagable Damagable =>
            _damagable ??= ContainerState.Target != null ? ContainerState.Target.Targetable as IDamagable : null;

        protected override void EnableBlock()
        {
            base.EnableBlock();

            _time = Time.realtimeSinceStartup;
        }

        public override void UpdateBlock()
        {
            if (ContainerState.Target == null)
            {
                _time = Time.realtimeSinceStartup;
                
                return;
            }
            
            float delta = Time.realtimeSinceStartup - _time;

            if (delta > (1f / _fireRate))
            {
                Fire();
                
                //reset time
                _time = Time.realtimeSinceStartup;
            }
        }

        private void Fire()
        {
            //play animation, sound and fx
            
            //check hit
            if (Physics.Raycast(Targeter.Muzzle.position, Targeter.Muzzle.forward, out RaycastHit hitInfo))
            {
                if (hitInfo.collider.TryGetComponent(out Target target) && target == ContainerState.Target)
                {
                    if (Damagable != null)
                    {
                        float damageSent = _damage * ContainerState.Target.Priority;
                    
                        DamageData hitData = new DamageData(new Dictionary<DamageType, float>
                            { { DamageType.Projectile, damageSent } }, Damagable.Damager, Damagable);
                    
                        if (Targeter.TryHit(target, hitData))
                        {
                            //hit
                            Debug.Log($"Hit {Damagable.Obj.name} with {damageSent} {DamageType.Projectile} damage");
                        }
                    }
                }
            }
        }
    }
}
