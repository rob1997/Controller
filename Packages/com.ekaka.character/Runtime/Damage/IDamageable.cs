using Core.Common;
using Sensors.Main;

namespace Character.Damage
{
    public interface IDamageable : ITargetable
    {
        public Vitality Vitality { get; }
        
        public Endurance Endurance { get; }
        
        public Damager Damager { get; }

        public float CurrentHealth => Vitality.CurrentValue;
        
        public float CurrentStamina => Endurance.CurrentValue;
        
        public float NormalizedHealth => CurrentHealth / Vitality.FullValue;
        
        public float NormalizedStamina => CurrentStamina / Endurance.FullValue;
        
        public bool IsDead => CurrentHealth <= 0;

        public GenericDictionary<DamageType, Resistance> Resistance => Vitality.Resistance;
        
        //initialize current health, either from data or vitality
        public float LoadCurrentHealth();
    }

    public static class DamageableWrapper
    {
        public static void InitializeDamageable(this IDamageable damageable)
        {
            damageable.Vitality.Initialize(damageable);
            
            damageable.Endurance.Initialize(damageable);
        }
        
        public static void TakeDamage(this IDamageable damageable, DamageData damageReceived)
        {
            damageable.Vitality.TakeDamage(damageReceived);
        }
        
        public static void GainHealth(this IDamageable damageable, float healthGained)
        {
            damageable.Vitality.GainHealth(healthGained);
        }
    }
}
