using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{
    #region Attack

    public delegate void Attack(Damagable damagable);

    public event Attack OnAttack;

    private void InvokeAttack(Damagable damagable)
    {
        OnAttack?.Invoke(damagable);
    }

    #endregion

    #region KillingBlow

    public delegate void KillingBlow(Damagable damagable);

    public event KillingBlow OnKillingBlow;

    private void InvokeKillingBlow(Damagable damagable)
    {
        OnKillingBlow?.Invoke(damagable);
    }

    #endregion

    public float Damage { get; private set; }
    
    public void CauseDamage(Damagable damagable)
    {
        bool isDead = damagable.IsDead;
        
        damagable.Attack(this);
        
        InvokeAttack(damagable);
        
        if (!isDead && damagable.IsDead)
        {
            InvokeKillingBlow(damagable);
        }
    }

    public void SetDamage(float damage)
    {
        Damage = damage;
    }
}