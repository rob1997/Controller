using System;
using System.Collections;
using System.Collections.Generic;
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
    
    public abstract class State<T> : StateBase where T : State<T>
    {
        [field: SerializeField] public StateTransition<T>[] Transitions { get; private set; }

        public override void TryExitState()
        {
            //can't exit function from a different state
            if (!IsEnabled)
            {
                Debug.LogWarning($"can't exit while {GetType().Name} is disabled");
                
                return;
            }
            
            foreach (var transition in Transitions)
            {
                if (transition.TryExitState())
                {
                    if (IsEnabled)
                    {
                        DisableState();
                    }
            
                    //check and enable exit states
                    if (transition.ExitStates != null)
                    {
                        foreach (var exitState in transition.ExitStates)
                        {
                            exitState.EnableState();
                        }
                    }
                }
            }
        }
    }
}