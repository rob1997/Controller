using System.Collections;
using System.Collections.Generic;
using Core.Game;
using UnityEngine;
using Weapon.Main;

namespace Weapon.Main
{
    [CreateAssetMenu(order = 0, fileName = nameof(Firearm), menuName = GameManager.StudioPrefix + "/Inventory/Firearm")]
    public class FirearmReference : RangedWeaponReference
    {
        [field: Space]
        
        [field: Header("Recoil")]
        
        [field: Tooltip("Weapon Recoil/the distance weapon actually moves when used/kickback")]
        [field: SerializeField] public Vector3 Recoil { get; private set; }
        [field: Tooltip("Weapon Recoil/the angle weapon actually rotates when used/kickback")]
        [field: SerializeField] public Vector3 RecoilEulerRotation { get; private set; }
        
        [field: Tooltip("Weapon Gain Pattern when used, not random to make it similar for all player")]
        [field: SerializeField] public Vector2[] RecoilPattern { get; private set; }

        [field: Tooltip("How long before the recoil pattern index times out and resets to index 0! usually resets when firing ends")]
        [field: SerializeField] public float RecoilPatternTimeout { get; private set; } = .5f;
        
        [field: Space]
        
        [field: Header("Fire")]
        
        [field: SerializeField] public bool SingleFire { get; private set; }

        [field: Range(0f, 1f)]
        [field: SerializeField]
        [field: Tooltip("By what factor is imprecision decreased, camera shake and recoil gain")]
        public float DownSightFactor { get; private set; } = .5f;
        
        [field: SerializeField] public float FireDuration { get; private set; }
        
        [field: SerializeField] public float CooldownDuration { get; private set; }
        
        [field: Space]
        
        [field: Header("Other")]
        
        [field: SerializeField] public Vector3 CameraShake { get; private set; }

        public float TotalDuration => FireDuration + CooldownDuration;
    }
}
