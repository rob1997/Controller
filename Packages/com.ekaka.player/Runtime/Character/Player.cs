using Character.Main;
using Data.Main;
using Data.Persistence;
using Data.Utils;
using Inventory.Main;
using UnityEngine;
using UnityEngine.InputSystem;

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

            if (GetController(out InventoryController inventoryController))
                SerializedData.DataModel.Bag = inventoryController.Bag;

            else
                Debug.LogWarning($"Can't save {nameof(Player)} Bag, {nameof(InventoryController)} not found");
        }
         
        #endregion
        
        public override float LoadCurrentHealth()
        {
            return SerializedData.DataModel.CurrentHealth;
        }
        
        private void Update()
        {
#if UNITY_EDITOR
            if (Keyboard.current.leftCtrlKey.isPressed && Keyboard.current.sKey.wasPressedThisFrame)
            {
                DataManager.Instance.Save();
            }
#endif
        }
    }
}
