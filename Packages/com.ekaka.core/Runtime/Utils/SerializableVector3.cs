using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Core.Utils
{
    [Serializable]
    public struct SerializableVector3
    {
        public float X;
    
        public float Y;
    
        public float Z;
    
        [JsonIgnore] public Vector3 ToVector3 => new Vector3(X, Y, Z);

        public SerializableVector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }
}
