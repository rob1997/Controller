using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using UnityEngine;
using UnityEngine.Serialization;

namespace NPC.Main
{
    public abstract class ContainerState<T> : State<T> where T : ContainerState<T>
    {
        [field: SerializeField] protected StateBlock<T>[] StateBlocks { get; private set; }

        public override void Initialize(NPCController controller)
        {
            //initialize state blocks first
            foreach (var block in StateBlocks)
            {
                block.InitializeBlock(this);
            }

            base.Initialize(controller);
        }

        public override void EnableState()
        {
            //enable base before stateBlocks
            EnableContainerState();

            //this will trigger a callback for stateBlocks to enable
            base.EnableState();
        }

        public override void UpdateState()
        {
            UpdateContainerState();

            UpdateStateBlocks();
        }

        public override void DisableState()
        {
            //disable base before stateBlocks
            DisableContainerState();

            //this will trigger a callback for stateBlocks to disable
            base.DisableState();
        }

        protected abstract void EnableContainerState();

        protected abstract void UpdateContainerState();

        protected abstract void DisableContainerState();

        private void UpdateStateBlocks()
        {
            foreach (var block in StateBlocks)
            {
                block.UpdateBlock();
            }
        }
    }
}
