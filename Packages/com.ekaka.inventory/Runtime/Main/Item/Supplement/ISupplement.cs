using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public interface ISupplement : IItem
    {
        int Count { get; }
    
        int Limit { get; }
    
        int Remainder { get; }

        void Add(int count);

        void Remove(int count);

        void Fill();

        void Resize(int count);
    }
}
