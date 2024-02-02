using System;
using System.Collections;
using System.Collections.Generic;
using NPC.Main;
using UnityEngine;

namespace NPC.Main
{
    public abstract class SubState<T> : StateBase where T : ContainerState<T>
    {
        public T ContainerState { get; private set; }

        public virtual void Initialize(ContainerState<T> containerState)
        {
            ContainerState = (T) containerState;

            containerState.OnStatusChanged += state =>
            {
                switch (state)
                {
                    case StateStatus.Enabled:
                        EnableState();
                        break;
                
                    case StateStatus.Disabled:
                        DisableState();
                        break;
                }
            };
        }
    }
}
