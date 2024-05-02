using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Game;
using Core.Common;
using Sensors.Main;
using UnityEngine;

namespace Damage.Main
{
    public interface IDamageable : ITargetable
    {
        public Vitality Vitality { get; }
        
        public Damager Damager { get; }

        public float CurrentHealth => Vitality.CurrentHealth;
        
        public float NormalizedHealth => CurrentHealth / Vitality.FullHealth;
        
        public bool IsDead => CurrentHealth <= 0;

        public GenericDictionary<DamageType, Resistance> Resistance => Vitality.Resistance;
        
        //initialize current health, either from data or vitality
        public float LoadCurrentHealth();
    }

    public static class DamagableWrapper
    {
        public static void InitializeDamagable(this IDamageable damageable)
        {
            damageable.Vitality.Initialize(damageable);
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
