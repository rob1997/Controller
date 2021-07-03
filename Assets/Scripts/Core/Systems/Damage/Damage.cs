using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Damage
{
    public enum DamageType
    {
        Fall,
        Fire,
        Cold
    }

    /// <summary>
    /// Damage type and Raw damage amount sent
    /// </summary>
    public Dictionary<DamageType, float> Hits { get; set; }
    
    public Damager Damager { get; set; }

    public Damagable Damagable
    {
        get => _damagable;

        private set
        {
            _damagable = value;

            CalculateDamageDealt();
        }
    }

    private Damagable _damagable;
    
    public float DamageDealt { get; private set; }

    public Damage(Dictionary<DamageType, float> hits, Damagable damagable)
    {
        Hits = hits;
        Damagable = damagable;
    }
    
    private void CalculateDamageDealt()
    {
        foreach (KeyValuePair<DamageType, float> hit in Hits)
        {
            Damagable.Resistance resistance = _damagable.resistance[hit.Key];

            if (!resistance.invulnerable) DamageDealt += hit.Value - (resistance.value * hit.Value);
        }
    }
}