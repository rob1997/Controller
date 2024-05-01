using UnityEngine;

namespace Damage.Main
{
    public abstract class DeathHandler : MonoBehaviour
    {
        public abstract void Apply(DamageData damage);
    }
}
