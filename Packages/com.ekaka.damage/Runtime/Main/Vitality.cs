using System;
using System.Linq;
using Core.Utils;
using UnityEngine;

namespace Damage.Main
{
    [Serializable]
    public struct Resistance
    {
        [field: SerializeField] public bool Invulnerable { get; private set; }
        
        [field: Range(0, 1)]
        [field: Tooltip("0 being will take all the damage sent 1 being invulnerable to damage")]
        [field: SerializeField] public float Value { get; private set; }
    }
    
    public class Vitality : MonoBehaviour
    {
        #region DamageTaken

        public delegate void DamageTaken(DamageData damage);

        public event DamageTaken OnDamageTaken;

        private void InvokeDamageTaken(DamageData damage)
        {
            OnDamageTaken?.Invoke(damage);
        }

        #endregion
    
        #region HeathGained

        public delegate void HeathGained(float gainedValue);

        public event HeathGained OnHeathGained;

        private void InvokeHeathGained(float gainedValue)
        {
            OnHeathGained?.Invoke(gainedValue);
        }

        #endregion
        
        #region HeathReceived

        public delegate void HeathReceived(float receivedValue);

        public event HeathReceived OnHeathReceived;

        private void InvokeHeathReceived(float receivedValue)
        {
            OnHeathReceived?.Invoke(receivedValue);
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

        [HideInInspector] public GenericDictionary<DamageType, Resistance> Resistance = GenericDictionary<DamageType, Resistance>
            .ToGenericDictionary(Utils.GetEnumValues<DamageType>().ToDictionary(d => d, d => new Resistance()));
    
        [field: Space]
        
        [field: SerializeField] public float FullHealth { get; private set; } = 100f;
        
        public float CurrentHealth { get; private set; }

        public float NormalizedHealth => CurrentHealth / FullHealth;
        
        public bool IsDead => CurrentHealth <= 0;

        public IDamagable Damagable { get; private set; }

        public void Initialize(IDamagable damagable)
        {
            Damagable = damagable;
            
            //load health from damagable
            CurrentHealth = Damagable.LoadCurrentHealth();
            
            //if current health is 0, it means it's a new Game or Damagable wasn't initialized before
            if (CurrentHealth <= 0)
            {
                CurrentHealth = FullHealth;
            }
            
            Debug.Log($"{Damagable.Obj.name} {nameof(Vitality)} initialized");
        }
        
        public void TakeDamage(DamageData damageReceived)
        {
            Debug.Log($"{damageReceived.DamageDealt} damage dealt to {Damagable.Obj.name}");
            
            float damageTaken = Mathf.Clamp(damageReceived.DamageDealt, 0, CurrentHealth);

            CurrentHealth -= damageTaken;

            Debug.Log($"{damageTaken} damage taken by {Damagable.Obj.name}");
            
            InvokeDamageTaken(damageReceived);
        
            damageReceived.Damager.InvokeDamageDealt(damageReceived);
        
            if (CurrentHealth <= 0)
            {
                Debug.Log($"{Damagable.Obj.name} Dead...");
                
                InvokeDeath(damageReceived);
            
                damageReceived.Damager.InvokeKillingBlow(damageReceived);
            }
        }

        public void GainHealth(float healthReceived)
        {
            Debug.Log($"{healthReceived} health received to {Damagable.Obj.name}");
            
            InvokeHeathReceived(healthReceived);
            
            float healthGained = Mathf.Clamp(healthReceived,0, FullHealth - CurrentHealth);
        
            CurrentHealth += healthGained;
        
            Debug.Log($"{healthGained} health gained by {Damagable.Obj.name}");
            
            InvokeHeathGained(healthGained);
        }
    }
}
