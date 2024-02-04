using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using Data.Main;
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
