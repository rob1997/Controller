using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Inventory.Main.Item
{
    public interface IItem
    {
        string Id { get; }
        
        string Title { get; }
        
        ItemReference Reference { get; }

        /// <summary>
        /// This isn't a perfect clone it's a memberwise/soft copy,
        /// moreover Item id is changed when cloned to avoid collision/duplicate
        /// </summary>
        /// <typeparam name="T">Type of Clone Item</typeparam>
        /// <returns>Cloned Item</returns>
        T Clone<T>() where T : IItem;
    }
}
