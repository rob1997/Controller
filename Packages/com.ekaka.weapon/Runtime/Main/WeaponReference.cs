using System.Collections;
using System.Collections.Generic;
using Core.Common;
using Character.Damage;
using Inventory.Main.Item;
using UnityEngine;

namespace Weapon.Main
{
    public abstract class WeaponReference : UsableReference
    {
        [field: SerializeField] public DamagePair[] Damage { get; private set; }
    }
}
