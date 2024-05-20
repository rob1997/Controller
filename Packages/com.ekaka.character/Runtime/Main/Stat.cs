using System;
using UnityEngine;

namespace Character.Main
{
    [Serializable]
    public class Stat
    {
        #region ValueReceived

        public delegate void ValueReceived(float amount);
        
        public event ValueReceived OnValueReceived;
        
        private void InvokeValueReceived(float amount)
        {
            OnValueReceived?.Invoke(amount);
        }

        #endregion
        
        #region ValueChanged

        public delegate void ValueChanged(float delta);
        
        public event ValueChanged OnValueChanged;

        private void InvokeValueChanged(float delta)
        {
            OnValueChanged?.Invoke(delta);
        }

        #endregion

        #region ValueFull

        public delegate void ValueFull();
        
        public event ValueFull OnValueFull;
        
        private void InvokeValueFull()
        {
            OnValueFull?.Invoke();
        }

        #endregion
        
        #region ValueDepleted

        public delegate void ValueDepleted();
        
        public event ValueDepleted OnValueDepleted;
        
        private void InvokeValueDepleted()
        {
            OnValueDepleted?.Invoke();
        }

        #endregion

        [field: SerializeField] public bool Immutable { get; private set; }
        
        [field: SerializeField, Tooltip("The lowest possible value of this stat before it's considered depleted.")]
        public float LowerLimit { get; private set; } = 0;
        
        [field: SerializeField] public float FullValue { get; private set; }
        
        public float CurrentValue { get; protected set; }

        public float NormalizedValue => CurrentValue / FullValue;

        public bool IsDepleted => CurrentValue <= LowerLimit;

        protected bool ChangeValue(float valueToChange, out float delta, bool silent = false)
        {
            if (!silent)
            {
                InvokeValueReceived(valueToChange);
            }
            
            delta = Immutable ? 0f : Mathf.Clamp(valueToChange, LowerLimit - CurrentValue, FullValue - CurrentValue);

            CurrentValue += delta;
            
            if (delta == 0)
            {
                return false;
            }
            
            if (!silent)
            {
                InvokeValueChanged(delta);
            }
                
            if (CurrentValue >= FullValue)
            {
                InvokeValueFull();
            }

            else if (CurrentValue <= LowerLimit)
            {
                InvokeValueDepleted();
            }

            return true;
        }
        
        protected bool GainValue(float valueToGain, out float valueGained, bool silent = false)
        {
            return ChangeValue(valueToGain, out valueGained, silent);
        }
        
        protected bool LoseValue(float valueToLose, out float valueLost, bool silent = false)
        {
            return ChangeValue(- valueToLose, out valueLost, silent);
        }
    }
}