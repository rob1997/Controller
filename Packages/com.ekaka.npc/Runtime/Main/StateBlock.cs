using System;
using System.Collections;
using System.Collections.Generic;
using NPC.Main;
using UnityEngine;

namespace NPC.Main
{
    /// <summary>
    /// For when a state can be made up of blocks.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class StateBlock<T> : MonoBehaviour where T : ContainerState<T>
    {
        public T ContainerState { get; private set; }

        public bool IsEnabled { get; private set; }
        
        public virtual void InitializeBlock(ContainerState<T> containerState)
        {
            ContainerState = (T) containerState;

            containerState.OnStatusChanged += isEnabled =>
            {
                if (isEnabled)
                {
                    EnableBlock();
                }
                
                else
                {
                    DisableBlock();
                }
            };
        }

        protected virtual void EnableBlock()
        {
            IsEnabled = true;
        }

        public abstract void UpdateBlock();

        protected virtual void DisableBlock()
        {
            IsEnabled = false;
        }
    }
}
