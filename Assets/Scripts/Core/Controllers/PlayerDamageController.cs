using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDamageController : DamageController
{
    public override void Initialize(Character character)
    {
        base.Initialize(character);
        
        character.Damagable.OnDeath += damage =>
        {
            GameManager.Instance.GetManager(out InputManager inputManager);
            
            inputManager.DisableAsset();
        };
    }
}