using System.Collections;
using System.Collections.Generic;
using Core.Common;
using UnityEngine;

public class HitEffect : MonoBehaviour, IPoolable
{
    [SerializeField] private ParticleSystem _effect;
    
    public void Renew()
    {
        gameObject.SetActive(true);
        
        _effect.Play();
    }

    public void Release()
    {
        gameObject.SetActive(false);
    }

    public void Retire()
    {
        Destroy(gameObject);
    }
}
