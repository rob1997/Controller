using System.Collections;
using System.Collections.Generic;
using Inventory.Main.Item;
using UnityEngine;

namespace Weapon.Main
{
    public abstract class WeaponAdapter<TItem, TReference> : UsableAdapter<TItem, TReference> 
        where TReference : WeaponReference where TItem : Weapon<TReference>
    {
        
    }
}
