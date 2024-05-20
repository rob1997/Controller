using System;
using Character.Main;
using Core.Game;
using UnityEngine;

namespace Character.Damage
{
    [Serializable]
    public class Endurance : Stat
    {
        [field: SerializeField, Tooltip("How many units of stamina is recovered per second.")]
        public float RecoveryRate { get; private set; } = 5f;

        public IDamageable Damageable { get; private set; }
        
        public void Initialize(IDamageable damageable)
        {
            CurrentValue = FullValue;
            
            Damageable = damageable;
        }

        public void RecoverStamina(float normalizedRate)
        {
            GainValue(RecoveryRate * normalizedRate * Time.deltaTime, out _, true);
        }
        
        public void DrainStamina(float normalizedRate)
        {
            LoseValue(RecoveryRate * normalizedRate * Time.deltaTime, out _, true);
        }
    }
}