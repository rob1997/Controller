using System;
using Core.Utils;
using UnityEngine;

namespace Core.Game
{
    public abstract class Manager<T> : Singleton<T>, IManager where T : Manager<T>
    {
        #region Ready

        public delegate void Ready();

        public event Ready OnReady;

        public void InvokeReady()
        {
            if (IsReady)
            {
                Debug.LogWarning($"{GetType()} is already ready");
                
                return;
            }
            
            OnReady?.Invoke();
            
            IsReady = true;
        }

        #endregion
        
        public bool IsReady { get; private set; }

        private void OnEnable()
        {
            Initialize();
            
            InvokeReady();
        }

        public abstract void Initialize();
    }
}
