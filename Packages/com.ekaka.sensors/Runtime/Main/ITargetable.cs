using System.Collections;
using System.Collections.Generic;
using Core.Game;
using UnityEngine;

namespace Sensors.Main
{
    public interface ITargetable : IComponent
    {
        public delegate void Hit(IHitData hitData);
        
        public TargetGroup TargetGroup { get; }

        public Targeter Targeter { get; }
        
        public Hit TargetHit { get; set; }
    }
    
    public static class ITargetableWrapper
    {
        public static void InitializeTargetable(this ITargetable targetable)
        {
            targetable.TargetGroup.Initialize(targetable);
        }
        
        public static void InvokeHit(this ITargetable targetable, IHitData hitData)
        {
            targetable.TargetHit?.Invoke(hitData);
        }
        
        public static Target GetTarget(this ITargetable targetable, Targeter targeter)
        {
            return targetable.TargetGroup.GetTarget(targeter);
        }
    }
}
