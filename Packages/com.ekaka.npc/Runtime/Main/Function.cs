using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NPC.Main
{
    public enum FunctionRunModeType
    {
        //runs every every frame on update loop
        Update,
        //runs every every frame on late update
        LateUpdate,
        //on fixedUpdate or every fixedTimeStep
        FixedUpdate,
        
        //run with delay, interval and frequency in FunctionRunMode
        Custom
    }
    
    [Serializable]
    public class FunctionRunMode
    {
        [field: SerializeField] public FunctionRunModeType RunModeType { get; private set; }
        
        [field: Tooltip("Per how many seconds is function run")]
        [field: SerializeField] public float Interval { get; private set; }
        
        [field: Tooltip("how many seconds to wait before start running")]
        [field: SerializeField] public float Delay { get; private set; }
        
        [field: Tooltip("how many time to run function, 0 means infinite")]
        [field: SerializeField] public int Frequency { get; private set; }

        //used for tracking run time interval and delay
        private float _time;
        //used for counting Delay then start running
        private bool _running;
        //current frequency
        private int _runCount;

        //run function Frequency times
        //check if functions has no frequency limit/runs forever
        public bool Completed => Frequency != 0 && _runCount >= Frequency;
        
        /// <summary>
        /// updates time with frame time step/DeltaTime
        /// </summary>
        /// <returns>if we should run function on this frame or not</returns>
        public bool UpdateTime()
        {
            _time += Time.deltaTime;
            
            if (_running)
            {
                //run interval reached
                if (_time >= Interval)
                {
                    //reset time
                    _time = 0;

                    //update/add to runCount
                    return UpdateFrequency();
                }
            }
            //delay
            else
            {
                if (_time > Delay)
                {
                    //reset time and start running
                    _time = 0;
                    
                    _running = true;
                    
                    //update/add to runCount
                    return UpdateFrequency();
                }
            }

            return false;
        }
        
        private bool UpdateFrequency()
        {
            //check if we're not running infinitely and add to run count
            if (Frequency != 0)
            {
                _runCount++;
            }

            return _runCount <= Frequency;
        }
    }
    
    public abstract class Function : FunctionBase
    {
        [field: Tooltip("If true there can only be one of this type per controller")]
        [field: SerializeField] public bool IsUnique { get; private set; } = false;

        //enables function on initialization
        [field: SerializeField] public bool RunOnInitialize { get; private set; } = false;

        //called/invoked/enabled on function break before complete case
        [field: SerializeField] public Function[] BreakFunctions { get; private set; }

        //called/invoked/enabled on completed
        [field: SerializeField] public Function[] NextFunctions { get; private set; }

        [field: SerializeField] public FunctionRunMode RunMode { get; private set; }

        public NPCController Controller { get; private set; }

        public virtual void Initialize(NPCController controller)
        {
            Controller = controller;

            if (RunOnInitialize)
            {
                EnableFunction();
            }

            else
            {
                DisableFunction();
            }
        }

        public virtual void CompleteFunction()
        {
            //can't complete function from a different state
            if (State != FunctionState.Enabled)
            {
                Debug.LogWarning($"can't complete {GetType().Name} {nameof(Function)} from {State} {nameof(State)}");
                
                return;
            }
            
            DisableFunction();

            if (NextFunctions != null && NextFunctions.Length > 0)
            {
                foreach (Function nextFunction in NextFunctions)
                {
                    nextFunction.EnableFunction();
                }
            }
        }

        protected void BreakFunction()
        {
            //can't break function from a different state
            if (State != FunctionState.Enabled)
            {
                Debug.LogWarning($"can't break {GetType().Name} {nameof(Function)} from {State} {nameof(State)}");
                
                return;
            }
            
            DisableFunction();
            
            //check and enable break functions
            if (BreakFunctions != null && BreakFunctions.Length > 0)
            {
                foreach (Function breakFunction in BreakFunctions)
                {
                    breakFunction.EnableFunction();
                }
            }
        }
    }
}