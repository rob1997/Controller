using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC.Utils
{
    [Serializable]
    public struct WayPoint
    {
        [field: SerializeField] public Transform Target { get; private set; }
    
        [field: SerializeField] public float Duration { get; private set; }

        public WayPoint(Transform target, float duration)
        {
            Target = target;

            Duration = duration;
        }
    }
}
