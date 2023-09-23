using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    [Serializable]
    public abstract class Supplement<T> : Item<T>, ISupplement where T : SupplementReference
    {
        [SerializeField] private int count;

        public int Count => count;

        public int Limit => reference.Limit;
    
        public int Remainder => Limit - count;

        public void Add(int amount)
        {
            count += Mathf.Clamp(amount, 0, Limit - count);
        }

        public void Remove(int amount)
        {
            count -= Mathf.Clamp(amount, 0, count);
        }

        public void Fill()
        {
            count = Limit;
        }
    
        public void Resize(int amount)
        {
            count = Mathf.Clamp(amount, 0 , Limit);
        }
    }
}
