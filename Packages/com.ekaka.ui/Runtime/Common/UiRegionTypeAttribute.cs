using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ui.Common
{
    [AttributeUsage(AttributeTargets.Field)]
    public class UiRegionTypeAttribute : PropertyAttribute
    {
        public UiRegionTypeAttribute()
        {
        
        }
    }
}
