using System;
using System.Collections;
using System.Collections.Generic;
using Core.Game;
using Core.Common;
using Data.Main;
using UnityEngine;

namespace Data.SceneLink
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
            
                return _referenceService?.GetComponentReference(Id);
            }
        }
    
        private void Initialize()
        {
            if (DataManager.Instance.IsReady)
            {
                SetReferenceService();
            }

            else
            {
                EventBus<ManagerReady<DataManager>>.Subscribe(SetReferenceService);
            }
        }
    
        void SetReferenceService()
        {
            _referenceService = DataManager.Instance.GlobalReferenceService;
        }
    }
}
