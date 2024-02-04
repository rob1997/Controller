using System;
using UnityEngine;

namespace NPC.Main
{
    public enum StateUpdateType
    {
        //state updates every every frame on update loop
        Update,
        //state updates every every frame on late update
        LateUpdate,
        //state updates on fixedUpdate or every fixedTimeStep
        FixedUpdate,
        
        //state updates with delay, interval and frequency in StateUpdate
        Custom
    }
    
    [Serializable]
    public class StateUpdate
    {
        [field: SerializeField] public StateUpdateType UpdateType { get; private set; }
        
        [field: Tooltip("Per how many seconds is state updated")]
        [field: SerializeField] public float Interval { get; private set; }
        
        [field: Tooltip("how many seconds to wait before start updating")]
        [field: SerializeField] public float Delay { get; private set; }
        
        [field: Tooltip("how many time to update state, 0 means infinite")]
        [field: SerializeField] public int Frequency { get; private set; }

        //used for tracking update time interval and delay
        private float _time;
        //used for counting Delay then start updating
        private bool _updating;
        //current frequency
        private int _updateCount;

        //update state Frequency times
        //check if states has no frequency limit/updates forever
        public bool Completed => Frequency != 0 && _updateCount >= Frequency;

        /// <summary>
        /// Just reset all cached values from any previous run
        /// </summary>
        public void ResetValues()
        {
            _time = 0;
            
            _updateCount = 0;

            _updating = false;
        }
        
        /// <summary>
        /// updates time with frame time step/DeltaTime
        /// </summary>
        /// <returns>if we should update state on this frame or not</returns>
        public bool UpdateTime()
        {
            _time += Time.deltaTime;
            
            if (_updating)
            {
                //update interval reached
                if (_time >= Interval)
                {
                    //reset time
                    _time = 0;

                    //update/add to updateCount
                    return UpdateFrequency();
                }
            }
            //delay
            else
            {
                if (_time > Delay)
                {
                    //reset time and start updating
                    _time = 0;
                    
                    _updating = true;
                    
                    //update/add to updateCount
                    return UpdateFrequency();
                }
            }

            return false;
        }
        
        private bool UpdateFrequency()
        {
            //check if we're not updating infinitely and add to update count
            if (Frequency != 0)
            {
                _updateCount++;
            }

            return _updateCount <= Frequency;
        }
    }
    
    public abstract class StateBase : MonoBehaviour
    {
        #region StatusChanged

        public delegate void StatusChanged(bool isEnabled);

        public event StatusChanged OnStatusChanged;

        #endregion

        [field: Tooltip("If true there can only be one of this type per controller")]
        [field: SerializeField] public bool IsUnique { get; private set; } = false;

        //enables state on initialization
        [field: SerializeField] public bool EnableOnInitialize { get; private set; } = false;
        
        [field: SerializeField] public StateUpdate StateUpdate { get; private set; }

        public StateUpdateType UpdateType => StateUpdate.UpdateType;
        
        public NPCController Controller { get; private set; }

        public bool IsEnabled { get; private set; }
        
        public bool IsCompleted { get; protected set; }
        
        public virtual void Initialize(NPCController controller)
        {
            Controller = controller;
        }
        
        public virtual void EnableState()
        {
            StateUpdate.ResetValues();

            IsCompleted = false;
            
            ChangeStatus(true);
        }

        public abstract void UpdateState();

        public virtual void DisableState()
        {
            ChangeStatus(false);
        }

        public abstract void TryExitState();
        
        private void ChangeStatus(bool isEnabled)
        {
            //same state
            if (IsEnabled == isEnabled)
            {
                return;
            }

            IsEnabled = isEnabled;

            //invoke event
            OnStatusChanged?.Invoke(IsEnabled);
        }
    }
}
