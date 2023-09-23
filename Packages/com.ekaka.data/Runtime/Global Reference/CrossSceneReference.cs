using System;
using System.Collections;
using System.Collections.Generic;
using Core.Game;
using Data.Main;
using UnityEngine;

namespace Data.GlobalReference
{
    [Serializable]
    public struct CrossSceneReference<T> where T : class
    {
        private GlobalReferenceService _referenceService;
    
        [field: SerializeField] public string Id { get; private set; }
    
        public bool IsLoaded => Reference != null;
    
        public T Reference => Component != null ? Component as T : null;
    
        public Component Component
        {
            get
            {
                if (_referenceService == null)
                {
                    Initialize();
                }
            
                return _referenceService.GetComponentReference(Id);
            }
        }
    
        private void Initialize()
        {
            if (GameManager.Instance.IsReady)
            {
                SetReferenceService();
            }

            else
            {
                GameManager.Instance.OnReady += SetReferenceService;
            }
        }
    
        void SetReferenceService()
        {
            _referenceService = DataManager.Instance.GlobalReferenceService;
        }
    }
}
