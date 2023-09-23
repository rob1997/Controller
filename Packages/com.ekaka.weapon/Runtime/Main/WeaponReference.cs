using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using Damage.Main;
using Inventory.Main.Item;
using UnityEngine;

namespace Weapon.Main
{
    public abstract class WeaponReference : UsableReference
    {
        [field: SerializeField] public DamagePair[] Damage { get; private set; }
    }
}
