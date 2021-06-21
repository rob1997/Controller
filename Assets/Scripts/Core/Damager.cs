using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Damager : MonoBehaviour
{
    #region DamageCaused

    public delegate void DamageCaused(float damageAmount);

    public event DamageCaused OnDamageCaused;

    private void InvokeDamageCaused(float damageAmount)
    {
        OnDamageCaused?.Invoke(damageAmount);
    }

    #endregion
    
    public void CauseDamage(Damagable damagable, float damageAmount)
    {
        damagable.TakeDamage(damageAmount);
        
        InvokeDamageCaused(damageAmount);
    }
}
