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
    
    [SerializeField] private Damagable damagable;

    [Space] public float value;
    
    private void Start()
    {
        fillImage.fillAmount = damagable.GetCurrentHealth() / damagable.GetFullHealth();
        SetHealthText();
        
        if (!damagable.invulnerable)
        {
            damagable.OnDamageDealt += amount =>
            {
                fillImage.fillAmount -= amount / damagable.GetFullHealth();

                SetHealthText();
            };
        }
        
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
            damagable.TakeDamage(value);
        }
        
        else if (Keyboard.current.numpadPlusKey.wasPressedThisFrame)
        {
            damagable.GainHealth(value);
        }
    }

    private void SetHealthText()
    {
        healthAmount.text = $"{Math.Round(damagable.GetCurrentHealth(), 2)}";
    }
}
