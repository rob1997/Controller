using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public enum UsageType
    {
        Primary,
        Secondary
    }

    public interface IUsableAdapter : IGearAdapter
    {
        Transform Holster { get; }

        Dictionary<UsageType, bool> CanUse { get; }

        void Use(UsageType usageType = UsageType.Primary);
        
        void Stop(UsageType usageType = UsageType.Primary);

        void AddUsage(Action usage, UsageType usageType = UsageType.Primary);
        
        void AddStoppage(Action usage, UsageType usageType = UsageType.Primary);
        
        void RemoveUsage(Action usage, UsageType usageType = UsageType.Primary);
        
        void RemoveStoppage(Action usage, UsageType usageType = UsageType.Primary);
    }
}
