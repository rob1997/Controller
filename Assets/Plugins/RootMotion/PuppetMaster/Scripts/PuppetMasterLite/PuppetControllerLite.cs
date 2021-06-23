using UnityEngine;
using System.Collections;

namespace RootMotion.Dynamics
{

    /// <summary>
    /// Dynamic pin, drag and mapping weight control based on collisions with objects.
    /// </summary>
    public class PuppetControllerLite : MonoBehaviour, ICollisionEventListener
    {

        [System.Serializable]
        public class Group
        {
            public string name;
            [Tooltip("The muscle groups to apply this pinWeightMlp and muscleWeightMlp to.")]
            public int[] indices = new int[0];
            [Range(0f, 1f)] public float pinWeightMlp = 0.5f;
            [Range(0f, 1f)] public float muscleWeightMlp = 0.5f;

            [Tooltip("When the puppet is touched, sets muscle Rigidbody drag to this value to reduce the rubber chicken effect.")]
            public float drag = 2f;
            [Tooltip("The time of blending in this script's effects when the puppet is touched.")]
            public float blendInTime = 0.05f;
            [Tooltip("The time of blending out this script's effects when the puppet is not touched any more.")]
            public float blendOutTime = 1f;

            public bool enabled { get; private set; }
            public float mappingWeight { get; private set; }

            private float dam = 0f;
            private float damTime = -100f;
            private float damV;
            private float map, mapV;
            
            public void TryDamage(Collision collision, CollisionEventBroadcaster broadcaster)
            {
                bool included = false;
                for (int i = 0; i < indices.Length; i++)
                {
                    if (broadcaster.muscle.index == indices[i])
                    {
                        included = true;
                        break;
                    }
                }
                if (!included) return;

                damTime = Time.time;
                enabled = true;
            }

            public void Update(PuppetMasterLite puppetMaster)
            {
                if (!enabled) return;

                // Dynamically adjust drag, pin and mapping weights based on collisions so we can have perfect animation until there is a collision
                //dragW = Mathf.MoveTowards(dragW, 1f, Time.deltaTime * 2f);

                bool unpinned = puppetMaster.pinWeight <= 0f;

                float damTarget = Time.time > damTime + 0.2f ? 0f : 1f;
                if (unpinned) damTarget = 1f;

                float mapTarget = damTarget;
                if (unpinned) mapTarget = 1f;

                float sDampTime = damTarget > dam ? blendInTime : blendOutTime;
                dam = Mathf.SmoothDamp(dam, damTarget, ref damV, sDampTime);
                if (damTarget < dam && dam < 0.001f) dam = 0f;

                float mDampTime = mapTarget > map ? blendInTime : blendOutTime;
                map = Mathf.SmoothDamp(map, mapTarget, ref mapV, mDampTime);
                if (mapTarget < map && map < 0.001f) map = 0f;

                if (unpinned) dam = Mathf.Min(dam, map);

                float d = unpinned ? 0f : drag * map;
                float angularD = unpinned ? 0.05f : drag * map;
                mappingWeight = Mathf.Lerp(0f, 1f, map);

                for (int i = 0; i < indices.Length; i++)
                {
                    int index = indices[i];
                    puppetMaster.muscles[index].pinWeightMlp = Mathf.Lerp(1f, pinWeightMlp, dam);
                    puppetMaster.muscles[index].muscleWeightMlp = Mathf.Lerp(1f, muscleWeightMlp, dam);

                    puppetMaster.muscles[index].rigidbody.drag = d;
                    puppetMaster.muscles[index].rigidbody.angularDrag = angularD;

                    //puppetMaster.muscles[index].mappingWeightMlp = mappingWeight;
                }

                if (dam <= 0f && map < 0f) enabled = false;
            }
        }

        public PuppetMasterLite puppetMaster;
        public LayerMask collisionLayers;
        
        [Tooltip("When the puppet is touched, sets pin weight and muscle weight values for these groups.")]
        public Group[] groups = new Group[0];       

        void Start()
        {
            foreach (MuscleLite m in puppetMaster.muscles)
            {
                var b = m.joint.gameObject.AddComponent<CollisionEventBroadcaster>();
                b.listener = this;
                b.muscle = m;
            }
        }

        private bool NeedToUpdate()
        {
            foreach (Group group in groups)
            {
                if (group.enabled) return true;
            }
            return false;
        }

        void FixedUpdate()
        {
            if (!NeedToUpdate()) return;

            float maxMappingWeight = 0f;

            foreach (Group group in groups)
            {
                group.Update(puppetMaster);
                maxMappingWeight = Mathf.Max(maxMappingWeight, group.mappingWeight);
            }

            foreach (MuscleLite m in puppetMaster.muscles)
            {
                m.mappingWeightMlp = maxMappingWeight;
            }
        }

        // Called by CollisionEventBroadcaster when it collides with something
        public void OnCollisionEnterEvent(Collision collision, CollisionEventBroadcaster broadcaster)
        {
            ProcessCollisionEvent(collision, broadcaster);
        }

        public void OnCollisionStayEvent(Collision collision, CollisionEventBroadcaster broadcaster)
        {
            ProcessCollisionEvent(collision, broadcaster);
        }

        private void ProcessCollisionEvent(Collision collision, CollisionEventBroadcaster broadcaster)
        {
            if (collision.collider.transform.root == transform) return;
            if (!LayerMaskExtensions.Contains(collisionLayers, collision.collider.gameObject.layer)) return;
            
            foreach (Group group in groups)
            {
                group.TryDamage(collision, broadcaster);
            }
        }

        public void OnCollisionExitEvent(Collision collision, CollisionEventBroadcaster broadcaster) {}
    }
}
