using System.Collections;
using System.Collections.Generic;
using Inventory.Main.Slot;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class Wearable<T> : Gear<T>, IWearable 
        where T : WearableReference
    {
        public WearableSlotType SlotType { get; }
    }
}
