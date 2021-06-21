using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damagable : MonoBehaviour
{
    [SerializeField] private float fullHealth;
    [SerializeField] private float startingHealth;

    [Space]
    
    public bool invulnerable;

    #region Attacked

    public delegate void Attacked(Damager damager);

    public event Attacked OnAttacked;

    private void InvokeAttacked(Damager damager)
    {
        OnAttacked?.Invoke(damager);
    }

    #endregion
    
    #region DamageDealt

    public delegate void DamageDealt(float damage);

    public event DamageDealt OnDamageDealt;

    private void InvokeDamageDealt(float damage)
    {
        OnDamageDealt?.Invoke(damage);
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

    public delegate void Death();

    public event Death OnDeath;

    private void InvokeDeath()
    {
        OnDeath?.Invoke();
    }

    #endregion

    #region FullHealthChanged

    public delegate void FullHealthChanged(float newFullHealth);

    public event FullHealthChanged OnFullHealthChanged;

    private void InvokeFullHealthChanged(float newFullHealth)
    {
        OnFullHealthChanged?.Invoke(newFullHealth);
    }

    #endregion

    #region CurrentHealthChanged

    public delegate void CurrentHealthChanged(float newCurrentHealth);

    public event CurrentHealthChanged OnCurrentHealthChanged;

    private void InvokeCurrentHealthChanged(float newCurrentHealth)
    {
        OnCurrentHealthChanged?.Invoke(newCurrentHealth);
    }

    #endregion
    
    private float _currentHealth;
    
    public bool IsDead => _currentHealth <= 0;

    private void Start()
    {
        _currentHealth = startingHealth;
    }

    public void Attack(Damager damager)
    {
        InvokeAttacked(damager);
        
        TakeDamage(damager.Damage);
    }
    
    public void TakeDamage(float damageTaken)
    {
        damageTaken = Mathf.Clamp(damageTaken, 0, _currentHealth);

        if (invulnerable)
        {
            damageTaken = 0;
        }
        
        _currentHealth -= damageTaken;
        
        InvokeDamageDealt(damageTaken);
        
        if (_currentHealth <= 0)
        {
            InvokeDeath();
        }
    }

    public void GainHealth(float healthGained)
    {
        healthGained = Mathf.Clamp(healthGained,0, fullHealth - _currentHealth);
        
        _currentHealth += healthGained;
        
        InvokeHeathGained(healthGained);
    }
    
    public void SetCurrentHealth(float newCurrentHealth)
    {
        if (newCurrentHealth <= 0 || newCurrentHealth > fullHealth)
        {
            Debug.LogError($"{newCurrentHealth} can't be <= zero or > {fullHealth}");
            
            return;
        }

        _currentHealth = Mathf.Clamp(newCurrentHealth, 0, fullHealth);
        
        InvokeCurrentHealthChanged(_currentHealth);
    }
    
    public void SetFullHealth(float newFullHealth)
    {
        if (newFullHealth <= 0)
        {
            Debug.LogError($"{newFullHealth} can't be <= zero");
            
            return;
        }

        if (newFullHealth < _currentHealth)
        {
            _currentHealth = newFullHealth;
        }

        fullHealth = newFullHealth;
        
        InvokeFullHealthChanged(fullHealth);
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
