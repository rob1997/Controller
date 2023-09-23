using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ui.Utils
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UiLayerTypeAttribute : PropertyAttribute
    {
        public UiLayerTypeAttribute()
        {
            
        }
    }
}
