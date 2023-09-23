using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using UnityEngine;

namespace NPC.Main
{
    public abstract class ContainerFunction<T> : Function where T : ContainerFunction<T>
    {
        [SerializeField] private SerializedValue<SubFunction<T>>[] _subFunctions;

        protected SubFunction<T>[] SubFunctions => _subFunctions.Select(s => s.Value).ToArray();

        public override void Initialize(NPCController controller)
        {
            //initialize sub functions first
            foreach (var subFunction in SubFunctions)
            {
                subFunction.Initialize(this);
            }

            base.Initialize(controller);
        }

        protected override void EnableFunction()
        {
            //enable base before subFunctions
            EnableContainerFunction();

            //this will trigger a callback for subFunctions to enable
            base.EnableFunction();
        }

        public override void Run()
        {
            RunContainerFunction();

            RunSubFunctions();
        }

        protected override void DisableFunction()
        {
            //disable base before subFunctions
            DisableContainerFunction();

            //this will trigger a callback for subFunctions to disable
            base.DisableFunction();
        }

        protected abstract void EnableContainerFunction();

        protected abstract void RunContainerFunction();

        protected abstract void DisableContainerFunction();

        private void RunSubFunctions()
        {
            foreach (var subFunction in SubFunctions)
            {
                subFunction.Run();
            }
        }
    }
}
