using System.Collections;
using System.Collections.Generic;
using Data.Common;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class ItemReference : AddressableScriptableObject
    {
        [field: SerializeField] public string Title { get; private set; }
    
        [field: SerializeField] public string Description { get; private set; }
    
        [field: SerializeField] public Texture2D Icon { get; private set; }
    
        [field: SerializeField] public float Weight { get; private set; }
    
        [field: SerializeField] public GameObject Prefab { get; private set; }
    }
}
