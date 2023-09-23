using System;
using System.Collections;
using Core.Game;
using Core.Utils;
using Inventory.Main;
using Inventory.Main.Item;
using Locomotion.Controllers;
using UnityEngine;
using Weapon.Utils;

namespace Weapon.Main
{
    public class FirearmAdapter : RangedWeaponAdapter<Firearm, FirearmReference>
    {
        [SerializeField] private Transform pivotTransform;
        
        [SerializeField] private Transform dockTransform;

        [Space]
        
        [SerializeField] private ParticleSystem muzzleEffect;
        
        [Space]
        
        [SerializeField] private Grabber.GrabTarget[] grabTargets;
        
        [SerializeField] private BarrelGroup barrels;
        
        private float _fireTime;
        
        private float _cooldownTime;
        
        private float _fireDuration;
        
        private float _cooldownDuration;

        private Transform _target;
        
        private Transform _dock;
        
        private Transform _cachedParent;

        private Grabber _grabber;
        
        private int _recoilIndex;
        
        private Coroutine _resetRecoilCoroutine;

        public bool IsAimingDown => !CanUse[UsageType.Secondary];
        
        protected override void CharacterReady()
        {
            base.CharacterReady();

            TryGetTarget();

            TryGetGrabber();
            
            AddUsage(Fire);
            
            AddStoppage(StopFiring);
            
            //set origin to shoulder beamer
            barrels.Initialize(this);
        }

        private void TryGetTarget()
        {
            if (Actor.GetController(out MotionController motionController))
            {
                if (motionController.IsReady) _target = motionController.Target;

                else motionController.OnReady += delegate { _target = motionController.Target; };
            }
        }

        private void TryGetGrabber()
        {
            if (Actor.GetController(out InventoryController inventoryController))
            {
                if (inventoryController.IsReady) _grabber = inventoryController.GetComponent<Grabber>();

                else inventoryController.OnReady += delegate { _grabber = inventoryController.GetComponent<Grabber>(); };
            }
        }
        
        public override void Equip()
        {
            base.Equip();

            _cachedParent = transform.parent;

            _grabber.Equip(item.SlotType);
        }

        public override void UnEquip()
        {
            base.UnEquip();

            _grabber.UnEquip(item.SlotType);

            muzzleEffect.gameObject.SetActive(false);
            
            _dock.LocalReset();

            _dock = null;

            transform.LocalReset(_cachedParent);
        }

        public override void EquippedCallback()
        {
            base.EquippedCallback();

            muzzleEffect.gameObject.SetActive(true);
            
            _dock = _grabber.Docks[item.SlotType];
            
            transform.SetParent(_dock);

            _dock.localPosition = dockTransform.localPosition;

            LookAtTarget();

            transform.SetLocalPositionAndRotation(pivotTransform.localPosition, pivotTransform.localRotation);

            _grabber.Equipped(item.SlotType, grabTargets);
        }

        public override void UnEquippedCallback()
        {
            
        }

        private void Update()
        {
            if (!IsEquipped) return;
            
            LookAtTarget();
            
            TryFireOnUpdate();
        }

        private void LookAtTarget()
        {
            if (_target != null && _dock != null)
            {
                _dock.rotation = Quaternion.LookRotation(_target.position - _dock.position);;
            }
        }

        private void TryFireOnUpdate()
        {
            //finished firing/hasn't started
            if (CanUse[UsageType.Primary]) return;
            
            Vector3 pivotLocalPosition = pivotTransform.localPosition;
            
            Quaternion pivotLocalRotation = pivotTransform.localRotation;
            
            if (_fireTime < _fireDuration)
            {
                float t = _fireTime / _fireDuration;
                
                transform.localPosition = Vector3.Lerp(pivotLocalPosition, pivotLocalPosition + Reference.Recoil, t);
                
                transform.localRotation = Quaternion.Lerp(pivotLocalRotation, pivotLocalRotation * Quaternion.Euler(Reference.RecoilEulerRotation), t);
                
                _fireTime += Time.deltaTime;

                if (_fireTime >= _fireDuration)
                {
                    transform.localPosition = pivotLocalPosition + Reference.Recoil;
                    
                    transform.localRotation = pivotLocalRotation * Quaternion.Euler(Reference.RecoilEulerRotation);
                    
                    _cooldownTime = 0;
                }
            }
            
            else if (_cooldownTime < _cooldownDuration)
            {
                float t = _cooldownTime / _cooldownDuration;
                
                transform.localPosition = Vector3.Lerp(pivotLocalPosition + Reference.Recoil, pivotLocalPosition, t);
                
                transform.localRotation = Quaternion.Lerp(pivotLocalRotation * Quaternion.Euler(Reference.RecoilEulerRotation), pivotLocalRotation, t);
                
                _cooldownTime += Time.deltaTime;

                if (_cooldownTime >= _cooldownDuration)
                {
                    transform.SetLocalPositionAndRotation(pivotLocalPosition, pivotLocalRotation);
                }
            }
            
            else
            {
                CanUse[UsageType.Primary] = true;

                //automatic/continuous
                if (!Reference.SingleFire) Use();
            }
        }
        
        private void Fire()
        {
            if (_resetRecoilCoroutine != null) StopCoroutine(_resetRecoilCoroutine);

            _resetRecoilCoroutine = null;
            
            CanUse[UsageType.Primary] = false;

            _fireDuration = Reference.FireDuration;
                
            _cooldownDuration = Reference.CooldownDuration;

            _fireTime = 0;
            
            muzzleEffect.Play();

            barrels.Fire();
        }
        
        private void StopFiring()
        {
            transform.SetLocalPositionAndRotation(pivotTransform.localPosition, pivotTransform.localRotation);

            if (_resetRecoilCoroutine != null) StopCoroutine(_resetRecoilCoroutine);

            _resetRecoilCoroutine = StartCoroutine(ResetRecoilIndex());
            
            barrels.StopFiring();
        }

        private IEnumerator ResetRecoilIndex()
        {
            yield return new WaitForSeconds(Reference.RecoilPatternTimeout);

            _recoilIndex = 0;

            _resetRecoilCoroutine = null;
        }

        public Vector3 GetShakeValue()
        {
            Vector3 shake = Reference.CameraShake;

            //aiming down sight
            if (IsAimingDown) shake *= Reference.DownSightFactor;
            
            return shake;
        }
        
        public Vector2 GetRecoilGain()
        {
            _recoilIndex++;

            if (_recoilIndex >= Reference.RecoilPattern.Length) _recoilIndex = 0;

            Vector2 recoilGain = Reference.RecoilPattern[_recoilIndex];
            
            //aiming down sight
            if (IsAimingDown) recoilGain *= Reference.DownSightFactor;
            
            return recoilGain;
        }
    }
}