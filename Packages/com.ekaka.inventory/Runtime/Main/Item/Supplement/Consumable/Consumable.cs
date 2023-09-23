using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class Consumable<T> : Supplement<T> where T : ConsumableReference
    {
    
    }
}
