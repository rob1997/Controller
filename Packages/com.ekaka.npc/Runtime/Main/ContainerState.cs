using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace NPC.Main
{
    public abstract class ContainerState<T> : State where T : ContainerState<T>
    {
        [field: SerializeField] protected SubState<T>[] SubStates { get; private set; }

        public override void Initialize(NPCController controller)
        {
            //initialize sub states first
            foreach (var subState in SubStates)
            {
                subState.Initialize(this);
            }

            base.Initialize(controller);
        }

        protected override void EnableState()
        {
            //enable base before subStates
            EnableContainerState();

            //this will trigger a callback for subStates to enable
            base.EnableState();
        }

        public override void UpdateState()
        {
            UpdateContainerState();

            UpdateSubStates();
        }

        protected override void DisableState()
        {
            //disable base before subStates
            DisableContainerState();

            //this will trigger a callback for subStates to disable
            base.DisableState();
        }

        protected abstract void EnableContainerState();

        protected abstract void UpdateContainerState();

        protected abstract void DisableContainerState();

        private void UpdateSubStates()
        {
            foreach (var subState in SubStates)
            {
                subState.UpdateState();
            }
        }
    }
}
