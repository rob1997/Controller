using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Utils
{
    [AttributeUsage(AttributeTargets.Field)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public string ConditionPath { get; private set; }
        
        public bool ShowIfCondition { get; private set; }
        
        public ShowIfAttribute(string conditionPath, bool showIfCondition = true)
        {
            ConditionPath = conditionPath;

            ShowIfCondition = showIfCondition;
        }
    }
}
