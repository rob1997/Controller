using System;
using System.Collections;
using System.Collections.Generic;
using Locomotion.Utils;
using UnityEngine;

namespace Locomotion.Utils
{
    public class AnimatorStateTracker : MonoBehaviour
    {
        #region StateStarted

        public delegate void StateStarted(int layerIndex, AnimatorStateInfo startedStateInfo);

        public event StateStarted OnStateStarted;

        private void InvokeStateStarted(int layerIndex, AnimatorStateInfo startedStateInfo)
        {
            OnStateStarted?.Invoke(layerIndex, startedStateInfo);
        }

        #endregion
        
        #region StateCompleted

        public delegate void StateCompleted(int layerIndex, AnimatorStateInfo completedStateInfo);

        public event StateCompleted OnStateCompleted;

        private void InvokeStateCompleted(int layerIndex, AnimatorStateInfo completedStateInfo)
        {
            OnStateCompleted?.Invoke(layerIndex, completedStateInfo);
        }

        #endregion

        [SerializeField] private Animator _animator;

        [field: Tooltip("should animation states be tracked, this shouldn't be always active since it's performance intensive")]
        [field: SerializeField] public bool IsActive { get; private set; }

        private AnimatorStateInfo[] _currentStates;

        private void Start()
        {
            _currentStates = new AnimatorStateInfo[_animator.layerCount];

            for (int i = 0; i < _animator.layerCount; i++)
            {
                _currentStates[i] = _animator.GetCurrentAnimatorStateInfo(i);
            }
        }

        private void Update()
        {
            if (IsActive) TrackStates();
        }

        private void TrackStates()
        {
            for (int i = 0; i < _animator.layerCount; i++)
            {
                AnimatorStateInfo currentState = _animator.GetCurrentAnimatorStateInfo(i);

                AnimatorStateInfo oldState = _currentStates[i];

                if (currentState.fullPathHash != oldState.fullPathHash)
                {
                    _currentStates[i] = currentState;

                    if (oldState.IsTracked()) InvokeStateCompleted(i, oldState);
                    
                    if (currentState.IsTracked()) InvokeStateStarted(i, currentState);
                }
            }
        }

        public void Activate()
        {
            IsActive = true;
        }

        public void Deactivate()
        {
            IsActive = false;
        }
    }
}