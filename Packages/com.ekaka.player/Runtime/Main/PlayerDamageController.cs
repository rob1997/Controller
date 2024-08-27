using Character.Damage;
using Character.Main;
using Core.Game;

namespace Player.Main
{
    public class PlayerDamageController : DamageController
    {
        public override void Initialize(Actor actor)
        {
            base.Initialize(actor);
            
            actor.Vitality.OnValueDepleted += delegate
            {
                GameManager.Instance.GameOver();
            };
        }
    }
}