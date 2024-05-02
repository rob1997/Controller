using System;
using Core.Common;
using UnityEngine;

namespace Core.Game
{
    public abstract class Manager<T> : Singleton<T>, IManager where T : Manager<T>
    {
        protected void InvokeReady()
        {
            if (IsReady)
            {
                Debug.LogWarning($"{GetType()} is already ready");
                
                return;
            }
            
            EventBus<ManagerReady<T>>.Invoke();
            
            IsReady = true;
        }

        public bool IsReady { get; private set; }

        private void OnEnable()
        {
            Initialize();
            
            InvokeReady();
        }

        public abstract void Initialize();
    }
}
