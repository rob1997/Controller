using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Game;
using Core.Utils;
using Sensors.Main;
using UnityEngine;

namespace Damage.Main
{
    public interface IDamagable : ITargetable
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
        public static void InitializeDamagable(this IDamagable damagable)
        {
            damagable.Vitality.Initialize(damagable);
        }
        
        public static void TakeDamage(this IDamagable damagable, DamageData damageReceived)
        {
            damagable.Vitality.TakeDamage(damageReceived);
        }
        
        public static void GainHealth(this IDamagable damagable, float healthGained)
        {
            damagable.Vitality.GainHealth(healthGained);
        }
    }
}
