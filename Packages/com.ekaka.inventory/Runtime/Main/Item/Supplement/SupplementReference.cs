using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class SupplementReference : ItemReference
    {
        [field: SerializeField] public int Limit { get; private set; }
    }
}
