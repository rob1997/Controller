using System.Collections;
using System.Collections.Generic;
using Inventory.Main.Slot;
using UnityEngine;

namespace Inventory.Main.Item
{
    public interface IUsable : IGear
    {
        UsableSlotType SlotType { get; }
    }
}
