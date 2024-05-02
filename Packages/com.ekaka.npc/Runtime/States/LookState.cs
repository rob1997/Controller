using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Common;
using NPC.Common;
using NPC.Main;
using Sensors.Main;
using UnityEngine;

namespace NPC.States
{
    public class LookState : State<LookState>
    {
        [field: Space]
        
        [field: SerializeField] public float LookSpeed { get; private set; } = 25f;

        [field: SerializeField] public WayPoint[] WayPoints { get; private set; }

        [field: Tooltip("loop Look through wayPoints or complete function on index end")]
        [field: SerializeField]
        public bool Loop { get; private set; }

        //this is what rotates when looking
        [field: SerializeField] public Transform LookFrom { get; private set; }

        [field: Space] [field: SerializeField] public LayerMask LookForLayerMask { get; private set; }

        [field: SerializeField] public TagMask IgnoreTagMask { get; private set; }

        public Targeter Targeter { get; private set; }
        
        private int _index;

        //rotation time
        private float _time;

        //wayPoint wait duration
        private float _waitTime;

        private bool _isWaiting;

        //duration for rotation
        private float _duration;

        //cached start rotation
        //updated every index
        private Quaternion _initialRotation;

        private Quaternion _targetRotation;

        private WayPoint CurrentWayPoint
        {
            get
            {
                //if wayPoint exists
                if (WayPoints != null && _index >= 0 && _index <= WayPoints.MaxIndex())
                {
                    return WayPoints[_index];
                }

                else
                {
                    _index = 0;

                    return new WayPoint(LookFrom, 0f);
                }
            }
        }

        public override void Initialize(NPCController controller)
        {
            base.Initialize(controller);

            Targeter = controller.Actor.Targeter;
        }

        public override void EnableState()
        {
            base.EnableState();

            //reset values
            _index = 0;

            WayPointIndexUpdated();
        }

        public override void UpdateState()
        {
            //finished rotating to wayPoint and is now waiting
            if (_isWaiting)
            {
                _waitTime += Time.deltaTime;

                //update index
                if (_waitTime >= CurrentWayPoint.Duration)
                {
                    UpdateWayPointIndex();
                }
            }

            else
            {
                LookFrom.rotation = Quaternion.Lerp(_initialRotation, _targetRotation, _time / _duration);

                _time += Time.deltaTime;

                if (_time >= _duration)
                {
                    //check waitDuration and start waiting
                    if (CurrentWayPoint.Duration > 0)
                    {
                        _isWaiting = true;
                    }
                    //if no wait time then just update index
                    else
                    {
                        UpdateWayPointIndex();
                    }
                }
            }
        }

        private void UpdateWayPointIndex()
        {
            _index++;

            if (_index > WayPoints.MaxIndex())
            {
                if (Loop)
                {
                    _index = 0;
                }

                else
                {
                    IsCompleted = true;

                    return;
                }
            }

            WayPointIndexUpdated();
        }

        private void WayPointIndexUpdated()
        {
            //reset values
            _time = 0;

            _waitTime = 0;

            _isWaiting = false;

            _initialRotation = LookFrom.rotation;

            Transform target = CurrentWayPoint.Target;

            Transform parent = LookFrom.parent;

            _targetRotation = target != LookFrom
                ? target.rotation
                :
                //if no target is provided default rotation should be default forward rotation
                (parent != null ? parent.rotation * Quaternion.Euler(Vector3.zero) : Quaternion.Euler(Vector3.zero));

            float delta = Quaternion.Angle(_initialRotation, _targetRotation);

            _duration = delta / LookSpeed;
        }
    }
}