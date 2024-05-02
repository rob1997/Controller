using System;
using Data.Main;
using Data.Common;
using Inventory.Main;

namespace Player.Character
{
    [Serializable]
    public class PlayerDataModel : IDataModel
    {
        public SerializableVector3 Position;
    
        public float CurrentHealth;
        
        public Bag Bag;
    }
}
