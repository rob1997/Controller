using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Common
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
