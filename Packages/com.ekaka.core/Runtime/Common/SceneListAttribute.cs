using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    public class SceneListAttribute : PropertyAttribute
    {
        public SceneListAttribute()
        {
            
        }
    }
}
