using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using UnityEngine;
using Weapon.Main;

namespace Weapon.Main
{
    public class BarrelGroup : MonoBehaviour
    {
#if UNITY_EDITOR
        public bool drawGizmo;
#endif

        [field: Range(0, 1)]
        [field: SerializeField]
        public float Spread { get; private set; }

        [field: SerializeField] public float Timeout { get; private set; } = .5f;

        [field: SerializeField] public HitEffectSpawner HitEffectSpawner { get; private set; }
        
        [field: SerializeField] public float HitObjDestroyTimeout { get; private set; }
        
        private int _index;

        private readonly Vector2 _size = Vector2.one;

        private Barrel[] _barrels;

        private Coroutine _resetIndexCoroutine;

        public Barrel[] Barrels
        {
            get
            {
                if (_barrels.IsNullOrEmpty()) _barrels = GetComponentsInChildren<Barrel>();

                return _barrels;
            }
        }

        public FirearmAdapter Adapter { get; private set; }
        
        public void Initialize(FirearmAdapter adapter)
        {
            Adapter = adapter;
            
            _barrels = GetComponentsInChildren<Barrel>();
            
            foreach (var barrel in Barrels)
            {
                barrel.Initialize(adapter.Actor.Targeter.Muzzle);
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!drawGizmo) return;

            Vector2 quadrant = _size / 2f;

            Vector3 position = transform.position;

            Vector3 up = transform.up * (quadrant.y / 2);

            Vector3 right = transform.right * (quadrant.x / 2);

            //draw squares/boxes

            //top left
            Core.Utils.Utils.DrawRectangle(position + up - right, right.normalized, up.normalized, quadrant, Color.red);
            //top right
            Core.Utils.Utils.DrawRectangle(position + up + right, right.normalized, up.normalized, quadrant, Color.red);
            //bottom right
            Core.Utils.Utils.DrawRectangle(position - up + right, right.normalized, up.normalized, quadrant, Color.red);
            //bottom left
            Core.Utils.Utils.DrawRectangle(position - up - right, right.normalized, up.normalized, quadrant, Color.red);

            Gizmos.color = Color.green;

            for (int i = 0; i < transform.childCount; i++)
            {
                Gizmos.DrawSphere(transform.GetChild(i).position, .025f);
            }
        }
#endif

        public void TakeSnapshot()
        {
            foreach (var barrel in Barrels)
            {
                barrel.TakeSnapshot();
            }
        }

        public void Fire()
        {
            if (_resetIndexCoroutine != null) StopCoroutine(_resetIndexCoroutine);

            _resetIndexCoroutine = null;

            foreach (var barrel in _barrels)
            {
                barrel.Fire(_index);
            }

            _index++;
        }

        public void StopFiring()
        {
            if (_resetIndexCoroutine != null) StopCoroutine(_resetIndexCoroutine);

            _resetIndexCoroutine = StartCoroutine(ResetIndex());
        }

        private IEnumerator ResetIndex()
        {
            yield return new WaitForSeconds(Timeout);

            _index = 0;

            _resetIndexCoroutine = null;
        }
    }
}