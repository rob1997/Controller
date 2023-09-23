using System;
using System.Collections;
using System.Collections.Generic;
using NPC.Main;
using UnityEngine;

namespace NPC.Main
{
    public abstract class SubFunction<T> : FunctionBase where T : ContainerFunction<T>
    {
        public T ContainerFunction { get; private set; }

        public virtual void Initialize(ContainerFunction<T> containerFunction)
        {
            ContainerFunction = (T) containerFunction;

            containerFunction.OnStateChanged += state =>
            {
                switch (state)
                {
                    case FunctionState.Enabled:
                        EnableFunction();
                        break;
                
                    case FunctionState.Disabled:
                        DisableFunction();
                        break;
                }
            };
        }
    }
}
