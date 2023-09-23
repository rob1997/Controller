using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Core.Utils;
using UnityEngine;

namespace Inventory.Main.Item
{
    [Serializable]
    public abstract class Item<T> : IItem where T : ItemReference
    {
        [SerializeField] protected T reference;

        [SerializeField] [HideInInspector] private string id = Utils.NewGuid();

        public string Id
        {
            get
            {
                if (string.IsNullOrEmpty(id))
                {
                    Debug.LogError("Item Guid NullOrEmpty, Initializing...");
                    
                    id = Utils.NewGuid();
                }

                return id;
            }

            private set => id = value;
        }

        public string Title => reference.Title;

        public ItemReference Reference => reference;

        public TItem Clone<TItem>() where TItem : IItem
        {
            //soft clone/copy...maintain references
            Item<T> item = (Item<T>) MemberwiseClone();
            
            item.Id = Utils.NewGuid();
            
            return (TItem) (IItem) item;
        }
    }
}