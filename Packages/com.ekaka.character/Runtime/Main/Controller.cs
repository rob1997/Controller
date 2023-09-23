using System.Collections.Generic;
using Core.Game;
using UnityEngine;

namespace Character.Main
{
    public abstract class Controller : MonoBehaviour
    {
        #region Ready

        public delegate void Ready();

        public event Ready OnReady;

        public void InvokeReady()
        {
            if (IsReady)
            {
                Debug.LogError($"{nameof(GameManager)} already ready");
            }

            else
            {
                OnReady?.Invoke();

                IsReady = true;
            }
        }

        #endregion

        public Actor Actor
        {
            get 
            {
#if UNITY_EDITOR
                if (!Application.isPlaying) return GetComponentInParent<Actor>();
#endif
                return _actor;
            }

            private set
            {
                _actor = value;
            }
        }

        public bool IsReady { get; private set; } = false;
        
        private Actor _actor;
        
        public virtual void Initialize(Actor actor)
        {
            if (IsReady) return;
            
            Actor = actor;
        }
    }
}
