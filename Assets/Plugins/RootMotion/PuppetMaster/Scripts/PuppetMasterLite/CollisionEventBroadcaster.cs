using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RootMotion.Dynamics
{

    public interface ICollisionEventListener
    {
        void OnCollisionEnterEvent(Collision collision, CollisionEventBroadcaster broadcaster);
        void OnCollisionStayEvent(Collision collision, CollisionEventBroadcaster broadcaster);
        void OnCollisionExitEvent(Collision collision, CollisionEventBroadcaster broadcaster);
    }

    public class CollisionEventBroadcaster : MonoBehaviour
    {

        public ICollisionEventListener listener;
        public MuscleLite muscle;

        private void OnCollisionEnter(Collision collision)
        {
            if (listener == null) return;
            listener.OnCollisionEnterEvent(collision, this);
        }

        private void OnCollisionStay(Collision collision)
        {
            if (listener == null) return;
            listener.OnCollisionStayEvent(collision, this);
        }

        private void OnCollisionExit(Collision collision)
        {
            if (listener == null) return;
            listener.OnCollisionExitEvent(collision, this);
        }
    }
}
