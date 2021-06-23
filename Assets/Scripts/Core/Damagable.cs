using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Damagable : MonoBehaviour
{
    [Serializable]
    public struct Resistance
    {
        public bool invulnerable;
        
        [Range(0, 1)]
        [Tooltip("0 being will take all the damage sent 1 being invulnerable to damage")]
        public float value;
    }

    #region DamageTaken

    public delegate void DamageTaken(Damage damage);

    public event DamageTaken OnDamageTaken;

    private void InvokeDamageTaken(Damage damage)
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

    #region Death

    public delegate void Death(Damage damage);

    public event Death OnDeath;

    private void InvokeDeath(Damage damage)
    {
        OnDeath?.Invoke(damage);
    }

    #endregion
    
    [SerializeField] private float fullHealth;
    [SerializeField] private float startingHealth;

    [Space]
    
    public GenericDictionary<Damage.DamageType, Resistance> resistance = new GenericDictionary<Damage.DamageType, Resistance>();

    private float _currentHealth;
    
    public bool IsDead => _currentHealth <= 0;

    private void Awake()
    {
        _currentHealth = startingHealth;
    }

    private void Start()
    {
        Utils.GetEnumValues<Damage.DamageType>().ToList().ForEach(type =>
        {
            if (!resistance.ContainsKey(type))
            {
                resistance.Add(type, new Resistance());
            }
        });
    }

    public void TakeDamage(Damage damageReceived)
    {
        float damageTaken = Mathf.Clamp(damageReceived.DamageDealt, 0, _currentHealth);

        _currentHealth -= damageTaken;

        InvokeDamageTaken(damageReceived);
        
        damageReceived.Damager.InvokeDamageDealt(damageReceived);
        
        if (_currentHealth <= 0)
        {
            InvokeDeath(damageReceived);
            
            damageReceived.Damager.InvokeKillingBlow(damageReceived);
        }
    }

    public void GainHealth(float healthGained)
    {
        healthGained = Mathf.Clamp(healthGained,0, fullHealth - _currentHealth);
        
        _currentHealth += healthGained;
        
        InvokeHeathGained(healthGained);
    }

    public float GetCurrentHealth()
    {
        return _currentHealth;
    }

    public float GetFullHealth()
    {
        return fullHealth;
    }
}
