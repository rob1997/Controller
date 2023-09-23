using System;
using Character.Main;
using Core.Utils;
using Data.Persistence;
using UnityEngine;

namespace Player.Character
{
    public class Player : Actor, IStorable
    {
        #region Singleton
        
        public static Player Instance { get; private set; }

        private void Awake()
        {
            //make player a Singleton
            if (Instance != null)
            {
                Debug.LogError($"There's already a {nameof(Player)} {nameof(Instance)}");
                
                Destroy(gameObject);
                
                return;
            }

            else
            {
                Instance = this;
            }
        }

        #endregion

        #region Data-Storage

        [field: SerializeField] public DataWrapper<PlayerDataModel> SerializedData;

        public IDataWrapper Data => SerializedData;

        public void UpdateData()
        {
            SerializedData.DataModel.Position = transform.position.ToSerializableVector3();
            
            SerializedData.DataModel.CurrentHealth = Vitality.CurrentHealth;
        }
        
        #endregion
        
        public override float LoadCurrentHealth()
        {
            return SerializedData.DataModel.CurrentHealth;
        }
    }
}
