using System;
using System.Collections;
using Core.Game;
using UnityEngine;
using UnityEngine.Pool;

namespace Core.Utils
{
    public abstract class Spawner<T> : MonoBehaviour where T : Component, IPoolable
    {
        [field: SerializeField] public int DefaultPoolSize { get; private set; }

        [field: SerializeField] public int MaxPoolSize { get; private set; }

        private ObjectPool<T> _pool;

        public ObjectPool<T> Pool =>
            _pool ??= new ObjectPool<T>(Create, Renew, Release, Retire, true, DefaultPoolSize, MaxPoolSize);

        protected abstract T Create();

        public T Spawn()
        {
            return Pool.Get();
        }
        
        public T Spawn(Vector3 position, Quaternion rotation)
        {
            T instance = Spawn();

            instance.transform.SetPositionAndRotation(position, rotation);
            
            return instance;
        }
        
        public T Spawn(Vector3 position, Quaternion rotation, Transform parent)
        {
            T instance = Spawn(position, rotation);

            instance.transform.SetParent(parent);
            
            return instance;
        }
        
        public T Spawn(Vector3 position, Quaternion rotation, Transform parent, float timeout)
        {
            T instance = Spawn(position, rotation, parent);
            
            DeSpawn(instance, timeout);

            return instance;
        }

        public void DeSpawn(T instance)
        {
            Pool.Release(instance);
        }
        
        public void DeSpawn(T instance, float timeout)
        {
            StartCoroutine(WaitAndDeSpawn(instance, timeout));
        }

        private IEnumerator WaitAndDeSpawn(T instance, float timeout)
        {
            yield return new WaitForSeconds(timeout);

            DeSpawn(instance);
        }
        
        protected void Renew(T instance)
        {
            instance.Renew();
        }

        protected void Release(T instance)
        {
            instance.Release();
        }

        protected void Retire(T instance)
        {
            instance.Retire();
        }
        
        public void Clear()
        {
            Pool.Clear();
        }
        
        private void OnApplicationQuit()
        {
            if (_pool != null && _pool.CountActive > 0)
            {
                Pool.Dispose();
            }
        }
    }
}
