using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class WearableAdapter<TItem, TReference> : GearAdapter<TItem, TReference>, IWearableAdapter 
        where TItem : Wearable<TReference> where TReference : WearableReference
    {
        
    }
}
