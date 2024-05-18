
namespace Character.Damage
{
    public class DefaultDeathHandler : DeathHandler
    {
        public override void Apply(DamageData damage)
        {
            Destroy(damage.Damageable.Obj);
        }
    }
}
