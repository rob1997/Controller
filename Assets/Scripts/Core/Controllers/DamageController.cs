using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageController : Controller
{
    [SerializeField] protected bool takeFallDamage;
    
    [Tooltip("A velocity threshold if exceeded during falling will damage the character on landing")]
    [SerializeField] protected float fallDamageVelocityThreshold = 10f;
    
    [Tooltip("How much damage character takes based on velocity units exceeding fallDamageVelocityThreshold")]
    [SerializeField] protected float fallDamagePerUnit = 2f;

    private MotionController _motionController;

    public override void Initialize(Character character)
    {
        base.Initialize(character);

        character.GetController(out _motionController);

        if (character.damagable != null)
        {
            _motionController.OnGroundStateChange += grounded =>
            {
                if (grounded && takeFallDamage)
                {
                    float landingVelocity =  - _motionController.GetVelocity().y;
                    
                    if (landingVelocity > fallDamageVelocityThreshold)
                    {
                        character.damagable.TakeDamage((landingVelocity - fallDamageVelocityThreshold) * fallDamagePerUnit);
                    }
                }
            };
        }
    }
}
