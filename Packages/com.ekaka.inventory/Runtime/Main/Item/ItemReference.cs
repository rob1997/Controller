using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class ItemReference : ScriptableObject
    {
        [field: SerializeField] public string Title { get; private set; }
    
        [field: SerializeField] public string Description { get; private set; }
    
        [field: SerializeField] public Texture2D Icon { get; private set; }
    
        [field: SerializeField] public float Weight { get; private set; }
    
        [field: SerializeField] public GameObject Prefab { get; private set; }
    }
}
