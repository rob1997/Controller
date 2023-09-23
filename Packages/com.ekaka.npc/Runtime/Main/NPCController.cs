using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Character.Main;
using UnityEngine;

namespace NPC.Main
{
    public class NPCController : Controller
    {
        private Function[] _functions = { };

        private Function[] EnabledFunctions => _functions.Where(f => f.State == FunctionState.Enabled).ToArray();
        
        public override void Initialize(Actor actor)
        {
            base.Initialize(actor);

            Function[] functions = GetComponentsInChildren<Function>();
        
            foreach (Function function in functions)
            {
                if (AddFunction(function))
                {
                    function.Initialize(this);
                }
            }
        }

        protected bool AddFunction<T>(T function) where T : Function
        {
            Type type = function.GetType();
        
            //function already exists
            if (_functions.Any(f => f.GetType() == type && f.IsUnique))
            {
                Debug.LogWarning($"can't add, {type.Name} is a unique {nameof(Function)}");
            
                return false;
            }
        
            _functions = _functions.Append(function).ToArray();

            return true;
        }

        private void Update()
        {
            RunFunctions(FunctionRunModeType.Update);
            //update custom functions in update to make update time more accurate
            RunFunctions(FunctionRunModeType.Custom);
        }
        
        private void FixedUpdate()
        {
            RunFunctions(FunctionRunModeType.FixedUpdate);
        }

        private void LateUpdate()
        {
            RunFunctions(FunctionRunModeType.LateUpdate);
        }

        private void RunFunctions(FunctionRunModeType runModeType)
        {
            foreach (Function function in EnabledFunctions.Where(f => f.RunMode.RunModeType == runModeType))
            {
                switch (runModeType)
                {
                    case FunctionRunModeType.Custom:
                        
                        if (function.RunMode.UpdateTime())
                        {
                            function.Run();
                            //check if function is completed/run frequency times
                            if (function.RunMode.Completed)
                            {
                                function.CompleteFunction();
                            }
                        }
                        
                        break;
                
                    default:
                        
                        function.Run();
                        
                        break;
                }
            }
        }
    }
}
