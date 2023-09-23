using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class ConsumableAdapter<TItem, TReference> : SupplementAdapter<TItem, TReference> 
        where TItem : Consumable<TReference> where TReference : ConsumableReference
    {
    
    }
}
