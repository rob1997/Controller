using System.Collections;
using System.Collections.Generic;
using Core.Common;
using UnityEngine;

public class HitEffectSpawner : Spawner<HitEffect>
{
    [SerializeField] private HitEffect _hitObjPrefab;
    
    protected override HitEffect Create()
    {
        return Instantiate(_hitObjPrefab);
    }
}
