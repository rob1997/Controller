using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager
{
    #region DamageDealt

    public delegate void DamageDealt(Damage damage);

    public event DamageDealt OnDamageDealt;

    public void InvokeDamageDealt(Damage damage)
    {
        OnDamageDealt?.Invoke(damage);
    }

    #endregion

    #region KillingBlow

    public delegate void KillingBlow(Damage damage);

    public event KillingBlow OnKillingBlow;

    public void InvokeKillingBlow(Damage damage)
    {
        OnKillingBlow?.Invoke(damage);
    }

    #endregion
    
    public void DealDamage(Damage damage)
    {
        damage.Damager = this;
        
        damage.Damagable.TakeDamage(damage);
    }
}