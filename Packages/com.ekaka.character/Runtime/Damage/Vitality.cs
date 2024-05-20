using System;
using System.Linq;
using Character.Main;
using Core.Common;
using UnityEngine;

namespace Character.Damage
{
    [Serializable]
    public struct Resistance
    {
        [field: SerializeField] public bool Invulnerable { get; private set; }
        
        [field: ShowIf(nameof(Invulnerable), false)]
        [field: Range(0, 1)]
        [field: Tooltip("0 being will take all the damage sent 1 being invulnerable to damage")]
        [field: SerializeField] public float Value { get; private set; }
    }

    [Serializable]
    public class Vitality : Stat
    {
        #region DamageTaken

        public delegate void DamageTaken(DamageData damage);

        public event DamageTaken OnDamageTaken;

        private void InvokeDamageTaken(DamageData damage)
        {
            OnDamageTaken?.Invoke(damage);
        }

        #endregion
    
        #region Death
        
        public delegate void DeathDelegate(DamageData damage);

        public event DeathDelegate OnDeath;

        private void InvokeDeath(DamageData damage)
        {
            OnDeath?.Invoke(damage);

            if (!DeathOverrides.TryGetValue(damage.MaxDamageType, out DeathHandler death))
            {
                death = Death;
            }
            
            death.Apply(damage);
        }
        
        [field: SerializeField] public DeathHandler Death { get; private set; }

        [field: SerializeField, SerializedDictionary,
                Tooltip(
                    "If the highest killing blow hit damage type is not found in this dictionary, default death will be applied instead.")]
        public GenericDictionary<DamageType, DeathHandler> DeathOverrides { get; private set; }

        #endregion

        [field: SerializeField, SerializedDictionary,
                Tooltip(
                    "Resistance to specific Damage type, 0 - 1, 1 being invulnerable to damage.")]
        public GenericDictionary<DamageType, Resistance> Resistance { get; private set; }
    
        public IDamageable Damageable { get; private set; }

        public void Initialize(IDamageable damageable)
        {
            Damageable = damageable;
            
            //load health from damageable
            CurrentValue = Damageable.LoadCurrentHealth();
            
            //if current health is 0, it means it's a new Game or Damageable wasn't initialized before
            if (CurrentValue <= 0)
            {
                CurrentValue = FullValue;
            }
        }
        
        public void TakeDamage(DamageData damageReceived)
        {
            Debug.Log($"{damageReceived.DamageDealt} damage dealt to {Damageable.Obj.name}");
            
            if (LoseValue(damageReceived.DamageDealt, out float damageTaken))
            {
                Debug.Log($"{damageTaken} damage taken by {Damageable.Obj.name}");
            
                InvokeDamageTaken(damageReceived);
        
                damageReceived.Damager.InvokeDamageDealt(damageReceived);
        
                if (IsDepleted)
                {
                    Debug.Log($"{Damageable.Obj.name} Dead...");
                
                    InvokeDeath(damageReceived);
            
                    damageReceived.Damager.InvokeKillingBlow(damageReceived);
                }
            }
        }

        public void GainHealth(float healthReceived)
        {
            Debug.Log($"{healthReceived} health received to {Damageable.Obj.name}");

            if (GainValue(healthReceived, out float healthGained))
            {
                Debug.Log($"{healthGained} health gained by {Damageable.Obj.name}");
            }
        }
    }
}
