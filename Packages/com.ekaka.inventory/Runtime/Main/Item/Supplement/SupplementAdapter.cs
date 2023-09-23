using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class SupplementAdapter<TItem, TReference> : ItemAdapter<TItem, TReference>
        where TItem : Supplement<TReference> where TReference : SupplementReference
    {
        public override void Pick(bool picked, string message)
        {
            if (picked)
            {
                Debug.Log(message);

                if (item.Count == 0) Destroy(gameObject);
            }

            else
            {
                Debug.LogWarning(message);
            }
        }
    }
}
