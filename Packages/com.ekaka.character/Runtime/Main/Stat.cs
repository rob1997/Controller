using System;
using UnityEngine;

namespace Character.Main
{
    [Serializable]
    public class Stat
    {
        #region ValueReceived

        public delegate void ValueReceived(float delta);
        
        public event ValueReceived OnValueReceived;
        
        private void InvokeValueReceived(float delta)
        {
            OnValueReceived?.Invoke(delta);
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

        [field: SerializeField, Tooltip("The lowest possible value of this stat before it's considered depleted.")]
        public float LowerLimit { get; private set; } = 0;
        
        [field: SerializeField] public float FullValue { get; private set; }
        
        public float CurrentValue { get; protected set; }

        public float NormalizedValue => CurrentValue / FullValue;

        public bool IsDepleted => CurrentValue <= LowerLimit;
        
        protected void GainValue(float valueToGain)
        {
            CurrentValue += Mathf.Clamp(valueToGain, 0, FullValue - CurrentValue);
        }
        
        protected bool GainValue(float valueToGain, out float valueGained)
        {
            InvokeValueReceived(valueToGain);

            if (!GainValueSilently(valueToGain, out valueGained))
            {
                return false;
            }

            InvokeValueChanged(valueGained);
                
            if (CurrentValue >= FullValue)
            {
                InvokeValueFull();
            }

            return true;
        }
        
        protected bool GainValueSilently(float valueToGain, out float valueGained)
        {
            float oldValue = CurrentValue;

            GainValue(valueToGain);
            
            valueGained = CurrentValue - oldValue;
            
            if (valueGained <= 0)
            {
                return false;
            }

            return true;
        }
        
        protected void LoseValue(float valueToLose)
        {
            CurrentValue -= Mathf.Clamp(valueToLose, 0, CurrentValue - LowerLimit);
        }
        
        protected bool LoseValue(float valueToLose, out float valueLost)
        {
            InvokeValueReceived(-valueToLose);

            if (!LoseValueSilently(valueToLose, out valueLost))
            {
                return false;
            }
            
            InvokeValueChanged(-valueLost);

            if (CurrentValue <= LowerLimit)
            {
                InvokeValueDepleted();
            }

            return true;
        }

        protected bool LoseValueSilently(float valueToLose, out float valueLost)
        {
            float oldValue = CurrentValue;
            
            LoseValue(valueToLose);
            
            valueLost = oldValue - CurrentValue;

            if (valueLost <= 0)
            {
                return false;
            }

            return true;
        }
    }
}