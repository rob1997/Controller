using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class DamageController : Controller
{
    [SerializeField] protected bool takeFallDamage;
    
    [Tooltip("A velocity threshold if exceeded during falling will damage the character on landing")]
    [SerializeField] protected float fallDamageVelocityThreshold = 10f;
    
    [Tooltip("How much damage character takes based on velocity units exceeding fallDamageVelocityThreshold")]
    [SerializeField] protected float fallDamagePerUnit = 2f;

    [Space]
    
    public Ragdoll ragdoll;
    
    private MotionController _motionController;
    private AnimationController _animationController;
    
    private bool _getUpAnimationCompleted;
    private bool _balanceRegained;
    
    public override void Initialize(Character character)
    {
        base.Initialize(character);

        GameManager.Instance.GetManager(out InputManager inputManager);

        character.GetController(out _motionController);
        character.GetController(out _animationController);

        _animationController.OnStateCompleted += info =>
        {
            if (info.IsName(Constants.Animation.GetUpProneStateShortName) || info.IsName(Constants.Animation.GetUpSupineStateShortName))
            {
                _getUpAnimationCompleted = true;
                
                if (_balanceRegained)
                {
                    inputManager.EnableAsset();
                }
            }
        };
        
        character.Damagable.OnDeath += damage =>
        {
            ragdoll.Activate();
        };

        ragdoll.OnActivated += delegate
        {
            _getUpAnimationCompleted = false;
            _balanceRegained = false;
            
            inputManager.DisableAsset();
        };
        
        ragdoll.OnBalanceRegained += delegate
        {
            _balanceRegained = true;

            if (_getUpAnimationCompleted)
            {
                inputManager.EnableAsset();
            }
        };
        
        _motionController.OnGroundStateChange += grounded =>
        {
            if (grounded && takeFallDamage)
            {
                float landingVelocity =  - _motionController.GetVelocity().y;
                    
                if (landingVelocity > fallDamageVelocityThreshold)
                {
                    Damage fallDamage = new Damage
                        (new Dictionary<Damage.DamageType, float>
                    {
                        {Damage.DamageType.Fall, (landingVelocity - fallDamageVelocityThreshold) * fallDamagePerUnit}
                    }, character.Damagable);
                        
                    character.Damager.DealDamage(fallDamage);
                }
            }
        };
    }
}
