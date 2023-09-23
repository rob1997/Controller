using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using Inventory.Main.Slot;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class UsableReference : GearReference
    {
        [field: SerializeField] public UsableSlotType SlotType { get; private set; }

        [HideInInspector]
        public ClipOverride ClipOverride;
    }
}
