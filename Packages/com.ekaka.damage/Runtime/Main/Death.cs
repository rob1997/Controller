using UnityEngine;

namespace Damage.Main
{
    public abstract class Death : MonoBehaviour
    {
        public abstract void Apply(DamageData damage);
    }
}
