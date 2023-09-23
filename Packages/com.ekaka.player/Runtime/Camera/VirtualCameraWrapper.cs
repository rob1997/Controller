using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using Core.Utils;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Player.VirtualCamera
{
    [Serializable]
    [RequireComponent(typeof(CinemachineVirtualCamera))]
    public class VirtualCameraWrapper : MonoBehaviour
    {
        private CinemachineCameraOffset _cameraOffset;
        
        private MouseLook _mouseLook;

        private Vector3 _cachedOffset;
        
        private bool _hasLookAt;
        
        private bool _hasCameraOffset;
        
        #region Gain

        private Vector2 _lookGain;

        private float _lookGainTime;
        
        private float _lookGainDuration;

        #endregion
        
        #region Shake Variables

        private Vector3 _shakeGain;
        
        private float _shakeTickDuration;
        
        private float _shakeCooldownDuration;

        private float _shakeTickTime;
        
        private float _shakeCooldownTime;

        #endregion
        
        public CinemachineVirtualCamera VirtualCamera { get; private set; }
        
        private void Awake()
        {
            VirtualCamera = GetComponent<CinemachineVirtualCamera>();

            _cameraOffset = GetComponent<CinemachineCameraOffset>();

            _hasCameraOffset = _cameraOffset != null;

            TryGetLookAt();
        }

        private void TryGetLookAt()
        {
            if (VirtualCamera.LookAt != null) _mouseLook = VirtualCamera.LookAt.GetComponent<MouseLook>();

            else if (VirtualCamera.Follow != null) _mouseLook = VirtualCamera.Follow.GetComponent<MouseLook>();

            _hasLookAt = _mouseLook != null;
        }

        private void Update()
        {
            if (_hasCameraOffset) TryShakeOnUpdate();
            
            if (_hasLookAt) TryLookGainOnUpdate();
        }

        public void ResetOffset(Vector3 offset)
        {
            if (_cameraOffset != null) _cameraOffset.m_Offset = offset;
        }

        #region Look Gain
        
        private void TryLookGainOnUpdate()
        {
            if (_lookGainTime < _lookGainDuration)
            {
                _lookGainTime += Time.deltaTime;
                
                _mouseLook.Gain = _lookGain * Time.deltaTime;

                if (_lookGainTime >= _lookGainDuration)
                {
                    _mouseLook.Gain = Vector2.zero;
                }
            }
        }

        public void LookGain(Vector2 gain, float gainDuration, float multiplier = 1f)
        {
            if (!_hasLookAt)
            {
                Debug.LogError("No MouseLook Component on Follow or LookAt Transform Targets");
                
                return;
            }

            _lookGain = gain * multiplier;
            
            _lookGainDuration = gainDuration;

            _lookGainTime = 0;
        }

        #endregion
        
        #region Shake

        public void Shake(Vector3 gain, float tickDuration, float cooldownDuration, float multiplier = 1f)
        {
            if (!_hasCameraOffset)
            {
                Debug.LogError("No Camera Offset Extension Component on Virtual Camera");
                
                return;
            }

            if (_shakeTickTime >= _shakeTickDuration && _shakeCooldownTime >= _shakeCooldownDuration)
            {
                _cachedOffset = _cameraOffset.m_Offset;
            }
            
            _shakeGain = _cameraOffset.m_Offset + (gain * multiplier);

            _shakeTickDuration = tickDuration;
            
            _shakeCooldownDuration = cooldownDuration;

            _shakeTickTime = 0;
        }

        private void TryShakeOnUpdate()
        {
            if (_shakeTickTime < _shakeTickDuration)
            {
                _cameraOffset.m_Offset = 
                    Vector3.Lerp(_cachedOffset, _shakeGain, _shakeTickTime / _shakeTickDuration);
                
                _shakeTickTime += Time.deltaTime;

                if (_shakeTickTime >= _shakeTickDuration)
                {
                    _cameraOffset.m_Offset = _shakeGain;
                    
                    _shakeCooldownTime = 0;
                }
            }
            
            else if (_shakeCooldownTime < _shakeCooldownDuration)
            {
                _cameraOffset.m_Offset =
                    Vector3.Lerp(_shakeGain, _cachedOffset, _shakeCooldownTime / _shakeCooldownDuration);
                
                _shakeCooldownTime += Time.deltaTime;

                if (_shakeCooldownTime >= _shakeCooldownDuration)
                {
                    _cameraOffset.m_Offset = _cachedOffset;
                }
            }
        }

        #endregion
    }
}
