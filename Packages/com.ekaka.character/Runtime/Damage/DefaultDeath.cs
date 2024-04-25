using Damage.Main;

namespace Character.Damage
{
    public class DefaultDeath : Death
    {
        public override void Apply(DamageData damage)
        {
            Destroy(damage.Damagable.Obj);
        }
    }
}
