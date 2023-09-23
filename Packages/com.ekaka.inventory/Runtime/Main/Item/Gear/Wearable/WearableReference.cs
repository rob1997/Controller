using System.Collections;
using System.Collections.Generic;
using Inventory.Main.Slot;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class WearableReference : GearReference
    {
        [field: SerializeField] public WearableSlotType SlotType { get; private set; }
    }
}
