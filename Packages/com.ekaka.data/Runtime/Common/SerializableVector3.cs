using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.Common
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

    public static class SerializableVector3Extensions
    {
        public static SerializableVector3 ToSerializableVector3(this Vector3 value)
        {
            return new SerializableVector3(value.x, value.y, value.z);
        }
    }
}
