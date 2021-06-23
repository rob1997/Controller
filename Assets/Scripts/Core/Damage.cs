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
    /// Raw damage amount sent
    /// </summary>
    public float Amount { get; private set; }
    
    public DamageType Type { get; private set; }

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

    public Damage(float amount, DamageType type, Damagable damagable)
    {
        Amount = amount;
        Type = type;
        Damagable = damagable;
    }
    
    private void CalculateDamageDealt()
    {
        Damagable.Resistance resistance = _damagable.resistance[Type];
        
        if (resistance.invulnerable) DamageDealt = 0;

        else DamageDealt = Amount - (resistance.value * Amount);
    }
}