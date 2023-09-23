using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Damage.Main;
using NPC.Main;
using Sensors.Main;

namespace NPC.Functions
{
    public class ShootFunction : SubFunction<TargetFunction>
    {
        [Tooltip("How many shots per second")]
        [SerializeField] private float _fireRate;

        [SerializeField] private float _damage;

        private float _time;

        private Targeter Targeter => ContainerFunction.Targeter;

        private IDamagable _damagable;
        
        protected override void EnableFunction()
        {
            base.EnableFunction();
            
            _damagable = ContainerFunction.Target.Targetable as IDamagable;
            
            _time = Time.realtimeSinceStartup;
        }

        public override void Run()
        {
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
                if (hitInfo.collider.TryGetComponent(out Target target) && target == ContainerFunction.Target)
                {
                    if (_damagable != null)
                    {
                        float damageSent = _damage * ContainerFunction.Target.Priority;
                    
                        DamageData hitData = new DamageData(new Dictionary<DamageType, float>
                            { { DamageType.Projectile, damageSent } }, _damagable.Damager, _damagable);
                    
                        if (Targeter.TryHit(target, hitData))
                        {
                            //hit
                            Debug.Log($"Hit {_damagable.Obj.name} with {damageSent} {DamageType.Projectile} damage");
                        }
                    }
                }
            }
        }
    }
}
