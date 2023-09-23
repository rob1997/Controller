using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Utils
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ObjectTypeAttribute : PropertyAttribute
    {
        public Type Type { get; private set; }
    
        public ObjectTypeAttribute(Type type)
        {
            Type = type;
        }
    }
}
