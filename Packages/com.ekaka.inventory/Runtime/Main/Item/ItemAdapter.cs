using System;
using System.Collections;
using System.Collections.Generic;
using Character.Main;
using UnityEngine;

namespace Inventory.Main.Item
{
    public abstract class ItemAdapter<TItem, TReference> : MonoBehaviour, IItemAdapter 
        where TItem : Item<TReference> where TReference : ItemReference
    {
        [SerializeField] protected TItem item;

        private TReference _reference;

        [field: SerializeField] [field: HideInInspector] 
        public Actor Actor { get; private set; }
        
        [field: SerializeField] [field: HideInInspector] 
        public bool Initialized { get; private set; }
        
        protected TReference Reference
        {
            get
            {
                if (_reference != null) return _reference;

                _reference = (TReference) item?.Reference;
                
                return _reference;
            }
        }

        public IItem Item => item;

        public GameObject Obj => this != null ? gameObject : null;

        public void StartWith(IItem iItem, Actor actor)
        {
            item = (TItem) iItem;

            Actor = actor;
        }

        private void Start()
        {
            //dry initialize (works with start with)
            if (!Initialized && (item != null || Actor != null))
            {
                Initialize(item, Actor);
            }
        }

        public virtual void Initialize(IItem iItem, Actor actor)
        {
            if (Initialized)
            {
                return;
            }

            else
            {
                Initialized = true;
            }
            
            item = (TItem) iItem;

            Actor = actor;

            if (Actor == null) Dropped();

            else
            {
                if (Actor.IsReady) CharacterReady();

                else Actor.OnReady += CharacterReady;
            }
        }

        protected abstract void CharacterReady();

        private void Dropped()
        {
            Collider[] colliders = GetComponents<Collider>();

            foreach (var c in colliders) c.enabled = true;

            if (TryGetComponent(out Rigidbody rBody)) rBody.isKinematic = false;
        }
        
        public void Focus()
        {
            Debug.Log($"Item {item.Title} in Focus");
        }

        public abstract void Pick(bool added, string message);
    }
}
