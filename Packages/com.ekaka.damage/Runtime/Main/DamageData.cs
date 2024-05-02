using System;
using System.Collections.Generic;
using System.Linq;
using Sensors.Main;
using UnityEngine;

namespace Damage.Main
{
    //!!!INDEXES MUST BE UNIQUE
    //!!!NEVER CHANGE THE INDEX NUMBERS, CAUSES A SHUFFLE ISSUE ON CUSTOM EDITOR
    public enum DamageType
    {
        Fall = 0,
        Cold = 1,
        Fire = 2,
        Magic = 3,
        Poison = 4,
        Projectile = 5,
    }

    [Serializable]
    public struct DamagePair
    {
        [field: SerializeField] public DamageType Type { get; private set; }
        
        [field: SerializeField] public float Value { get; private set; }
    }
    
    [Serializable]
    public struct DamageData : IHitData
    {
        /// <summary>
        /// Damage type and Raw damage amount sent
        /// </summary>
        public Dictionary<DamageType, float> Hits { get; private set; }
    
        public Damager Damager { get; private set; }

        public IDamageable Damageable { get; private set; }
    
        public float DamageDealt { get; private set; }

        public DamageType MaxDamageType { get; private set; }
        
        public DamageData(Dictionary<DamageType, float> hits, Damager damager, IDamageable damageable)
        {
            Hits = hits;
            
            Damageable = damageable;

            Damager = damager;

            //calculate damage dealt
            DamageDealt = 0;
            
            foreach (var hitPair in Hits)
            {
                Resistance resistance = Damageable.Resistance[hitPair.Key];

                if (!resistance.Invulnerable) DamageDealt += hitPair.Value - (resistance.Value * hitPair.Value);
            }
            
            MaxDamageType = Hits.Aggregate((h, l) => h.Value > l.Value ? h : l).Key;
        }
    }
}