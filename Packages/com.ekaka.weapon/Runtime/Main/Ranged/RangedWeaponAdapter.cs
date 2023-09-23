
namespace Weapon.Main
{
    public abstract class RangedWeaponAdapter<TItem, TReference> : WeaponAdapter<TItem, TReference> 
        where TItem : RangedWeapon<TReference> where TReference : RangedWeaponReference
    {
        
    }
}