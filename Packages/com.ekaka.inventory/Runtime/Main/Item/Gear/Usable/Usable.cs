using System;
using System.Collections;
using System.Collections.Generic;
using Inventory.Main.Slot;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class Usable<T> : Gear<T>, IUsable where T : UsableReference
    {
        public UsableSlotType SlotType => reference.SlotType;
    }
}
