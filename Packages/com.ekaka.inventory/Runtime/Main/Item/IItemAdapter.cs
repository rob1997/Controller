using System;
using System.Collections;
using System.Collections.Generic;
using Character.Main;
using UnityEngine;

namespace Inventory.Main.Item
{
    public interface IItemAdapter
    {
        IItem Item { get; }
        
        GameObject Obj { get; }
        
        Actor Actor { get; }

        bool Initialized { get; }
        
        //just initializes/assigns the values
        void StartWith(IItem item, Actor actor);
        
        /// <summary>
        /// Initialize adapter
        /// </summary>
        /// <param name="item">item for the adapter</param>
        /// <param name="actor">character holding the item, if null means item is dropped</param>
        void Initialize(IItem item, Actor actor);

        void Focus();

        void Pick(bool picked, string message);
    }
}
