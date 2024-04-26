using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Utils
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SerializedDictionaryAttribute : PropertyAttribute
    {
        public SerializedDictionaryAttribute()
        {
            
        }
    }
}
