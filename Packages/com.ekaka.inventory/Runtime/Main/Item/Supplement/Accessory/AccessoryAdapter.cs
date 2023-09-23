using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class AccessoryAdapter<TItem, TReference> : SupplementAdapter<TItem, TReference> 
        where TItem : Accessory<TReference> where TReference : AccessoryReference
    {
        
    }
}
