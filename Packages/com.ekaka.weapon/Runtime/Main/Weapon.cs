using System.Collections;
using System.Collections.Generic;
using Inventory.Main.Item;
using UnityEngine;

namespace Weapon.Main
{
    public abstract class Weapon<T> : Usable<T> where T : UsableReference
    {
        
    }
}
