using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ui.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UiLayerTypeAttribute : PropertyAttribute
    {
        public UiLayerTypeAttribute()
        {
            
        }
    }
}
