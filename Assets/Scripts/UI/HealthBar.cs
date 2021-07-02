using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private TextMeshProUGUI healthAmount;
    
    [Space]
    
    [SerializeField] private Player player;

    [Space] public float value;
    
    private void Start()
    {
        Damagable damagable = player.Damagable;
        
        fillImage.fillAmount = damagable.GetCurrentHealth() / damagable.GetFullHealth();
        
        SetHealthText();
        
        damagable.OnDamageTaken += damage =>
        {
            fillImage.fillAmount -= damage.DamageDealt / damagable.GetFullHealth();

            SetHealthText();
        };
        
        damagable.OnHeathGained += amount =>
        {
            fillImage.fillAmount += amount / damagable.GetFullHealth();
            
            SetHealthText();
        };
    }

    private void Update()
    {
        if (Keyboard.current.numpadMinusKey.wasPressedThisFrame)
        {
            Damage fallDamage = new Damage
            (new Dictionary<Damage.DamageType, float>
            {
                {Damage.DamageType.Fall, value} 
            }, player.Damagable);
                        
            player.Damager.DealDamage(fallDamage);
        }
        
        else if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
        {
            player.Damagable.GainHealth(value);
        }
    }

    private void SetHealthText()
    {
        healthAmount.text = $"{Math.Round(player.Damagable.GetCurrentHealth(), 2)}";
    }
}