using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public delegate void Equipped();
    
    public delegate void UnEquipped();
    
    public interface IGearAdapter : IItemAdapter
    {
        bool IsEquipped { get; }
        
        Equipped Equipped { get; set; }
        
        UnEquipped UnEquipped { get; set; }

        IGear Gear { get; }
        
        void Equip();
        
        void UnEquip();

        void EquippedCallback();
        
        void UnEquippedCallback();
    }
}
