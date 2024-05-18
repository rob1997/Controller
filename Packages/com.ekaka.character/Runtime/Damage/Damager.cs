namespace Character.Damage
{
    public class Damager
    {
        #region DamageDealt

        public delegate void DamageDealt(DamageData damage);

        public event DamageDealt OnDamageDealt;

        public void InvokeDamageDealt(DamageData damage)
        {
            OnDamageDealt?.Invoke(damage);
        }

        #endregion

        #region KillingBlow

        public delegate void KillingBlow(DamageData damage);

        public event KillingBlow OnKillingBlow;

        public void InvokeKillingBlow(DamageData damage)
        {
            OnKillingBlow?.Invoke(damage);
        }

        #endregion
    
        public void DealDamage(DamageData damage)
        {
            damage.Damageable.TakeDamage(damage);
        }
    }
}