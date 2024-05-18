using UnityEngine;

namespace Character.Damage
{
    public abstract class DeathHandler : MonoBehaviour
    {
        public abstract void Apply(DamageData damage);
    }
}
