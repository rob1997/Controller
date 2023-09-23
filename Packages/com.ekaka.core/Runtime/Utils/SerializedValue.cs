using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.Utils
{
    [Serializable]
    public struct SerializedValue<T> where T : Component
    {
        [field: SerializeField] public T Value { get; private set; }
    }
}
