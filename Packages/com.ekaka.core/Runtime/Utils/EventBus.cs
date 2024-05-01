using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Utils
{
    public abstract class EventBase<TDelegate, TConcrete> where TDelegate : Delegate where TConcrete : EventBase<TDelegate, TConcrete>, new()
    {
        private readonly HashSet<TDelegate> _listeners = new HashSet<TDelegate>();

        protected TDelegate eventHandler;

        private readonly List<TDelegate> _oneTimeListeners = new List<TDelegate>();
        
        protected static TConcrete Instance { get; private set; } = new TConcrete();
        
        protected virtual void AddListener(TDelegate handler)
        {
            if (_listeners.Contains(handler))
            {
                throw new InvalidOperationException("Attempting to subscribe a method that is already subscribed.");
            }

            _listeners.Add(handler);
        }

        protected virtual void RemoveListener(TDelegate handler)
        {
            if (!_listeners.Contains(handler))
            {
                throw new InvalidOperationException("Attempting to unsubscribe a method that is not subscribed.");
            }

            _listeners.Remove(handler);
            
            if (_oneTimeListeners.Contains(handler))
            {
                _oneTimeListeners.Remove(handler);
            }
        }

        private void ListenOneTime(TDelegate handler)
        {
            AddListener(handler);
            
            _oneTimeListeners.Add(handler);
        }
        
        protected virtual void ClearOneTimeListeners()
        {
            foreach (TDelegate listener in _oneTimeListeners.ToList())
            {
                RemoveListener(listener);
            }
            
            _oneTimeListeners.Clear();
        }
        
        /// <summary>
        /// Subscribe to event.
        /// </summary>
        /// <param name="handler">Listener method subscribing to event.</param>
        public static void Subscribe(TDelegate handler)
        {
            Instance.AddListener(handler);
        }
        
        /// <summary>
        /// Subscribe to event for a single call.
        /// </summary>
        /// <param name="handler">Listener method subscribing to event.</param>
        public static void SubscribeOneTime(TDelegate handler)
        {
            Instance.ListenOneTime(handler);
        }
        
        /// <summary>
        /// Unsubscribe from event.
        /// </summary>
        /// <param name="handler">Listener method unsubscribing to event.</param>
        public static void Unsubscribe(TDelegate handler)
        {
            Instance.RemoveListener(handler);
        }
        
        /// <summary>
        /// Try to Unsubscribe from event.
        /// This won't throw an exception if the listener is not subscribed.
        /// </summary>
        /// <param name="handler">Listener method subscribed to event.</param>
        public static void TryUnsubscribe(TDelegate handler)
        {
            try
            {
                Instance.RemoveListener(handler);
            }
            catch (InvalidOperationException)
            {
                Debug.LogWarning("Attempting to unsubscribe a method that is not subscribed.");
            }
        }
    }

    public class EventBus<T> : EventBase<Action<T>, EventBus<T>> where T : IEventParams
    {
        private readonly Dictionary<Action, Action<T>> _actionMap = new Dictionary<Action, Action<T>>();

        private readonly List<Action> _oneTimeListeners = new List<Action>();
        
        protected override void AddListener(Action<T> handler)
        {
            base.AddListener(handler);

            eventHandler += handler;
        }
        
        private void AddListener(Action handler)
        {
            if (_actionMap.ContainsKey(handler))
            {
                throw new InvalidOperationException("Attempting to subscribe a method that is already subscribed.");
            }
            
            void Handler(T arg)
            {
                handler();
            }

            eventHandler += Handler;
            
            _actionMap.Add(handler, Handler);
        }

        private void ListenOneTime(Action handler)
        {
            AddListener(handler);
            
            _oneTimeListeners.Add(handler);
        }
        
        /// <summary>
        /// Subscribe to event.
        /// </summary>
        /// <param name="handler">Listener method subscribing to event.</param>
        public static void Subscribe(Action handler)
        {
            Instance.AddListener(handler);
        }
        
        /// <summary>
        /// Subscribe to event for a single call.
        /// </summary>
        /// <param name="handler">Listener method subscribing to event.</param>
        public static void SubscribeOneTime(Action handler)
        {
            Instance.ListenOneTime(handler);
        }
        
        protected override void RemoveListener(Action<T> handler)
        {
            base.RemoveListener(handler);

            eventHandler -= handler;
        }
        
        private void RemoveListener(Action handler)
        {
            if (_actionMap.TryGetValue(handler, out Action<T> action))
            {
                _actionMap.Remove(handler);
                
                if (_oneTimeListeners.Contains(handler))
                {
                    _oneTimeListeners.Remove(handler);
                }
                
                eventHandler -= action;
            }

            else
            {
                throw new InvalidOperationException("Attempting to unsubscribe a method that is not subscribed.");
            }
        }

        /// <summary>
        /// Unsubscribe from event.
        /// </summary>
        /// <param name="handler">Listener method subscribed to event.</param>
        public static void Unsubscribe(Action handler)
        {
            Instance.RemoveListener(handler);
        }
        
        /// <summary>
        /// Try to Unsubscribe from event.
        /// This won't throw an exception if the listener is not subscribed.
        /// </summary>
        /// <param name="handler">Listener method subscribed to event.</param>
        public static void TryUnsubscribe(Action handler)
        {
            try
            {
                Instance.RemoveListener(handler);
            }
            catch (InvalidOperationException)
            {
                Debug.LogWarning("Attempting to unsubscribe a method that is not subscribed.");
            }
        }

        private void Publish(T arg)
        {
            eventHandler?.Invoke(arg);
            
            ClearOneTimeListeners();
        }
        
        /// <summary>
        /// Invoke event.
        /// </summary>
        /// <param name="arg">Event argument.</param>
        public static void Invoke(T arg)
        {
            Instance.Publish(arg);
        }
        
        /// <summary>
        /// Invoke event.
        /// </summary>
        public static void Invoke()
        {
            Instance.Publish(default);
        }

        protected override void ClearOneTimeListeners()
        {
            base.ClearOneTimeListeners();
            
            foreach (Action listener in _oneTimeListeners.ToList())
            {
                RemoveListener(listener);
            }
            
            _oneTimeListeners.Clear();
        }
    }
    
    public interface IEventParams
    {
    }
}