using Damage.Main;

namespace Character.Damage
{
    public class DefaultDeathHandler : DeathHandler
    {
        public override void Apply(DamageData damage)
        {
            Destroy(damage.Damagable.Obj);
        }
    }
}
