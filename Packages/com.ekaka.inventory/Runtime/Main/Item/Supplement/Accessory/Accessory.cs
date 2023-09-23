using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class Accessory<T> : Supplement<T> where T : AccessoryReference
    {
        
    }
}
