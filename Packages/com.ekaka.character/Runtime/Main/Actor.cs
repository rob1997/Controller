using System.Linq;
using Core.Game;
using Core.Common;
using Character.Damage;
using Sensors.Main;
using UnityEngine;

namespace Character.Main
{
    public abstract class Actor : MonoBehaviour, IDamageable
    {
        #region Ready

        public delegate void Ready();

        public event Ready OnReady;

        private void InvokeReady()
        {
            if (IsReady)
            {
                Debug.LogError($"{nameof(GameManager)} already ready");
            }

            else
            {
                OnReady?.Invoke();

                IsReady = true;
            }
        }

        #endregion
        
        //Used to dispatch events during runtime
        //such as animation events that are only invoked on the GameObject containing the animator
        #region EventDispatched

        public delegate void EventDispatched(string label, params object[] args);

        public event EventDispatched OnEventDispatched;

        protected void InvokeEventDispatched(string label, params object[] args)
        {
            OnEventDispatched?.Invoke(label, args);
        }

        #endregion
        
        [field: SerializeField] public string Title { get; private set; }
        
        [field: TextArea]
        [field: SerializeField] public string Description { get; private set; }

        [field: Space]
        
        [field: SerializeField] public Animator Animator { get; private set; }
        
        private Controller[] _controllers = { };

        private CharacterController _characterController;

        public bool IsReady { get; private set; }

        public CharacterController CharacterController
        {
            get
            {
                if (_characterController != null) return _characterController;
                
                if (!TryGetComponent(out _characterController)) Debug.LogError("Character Controller Component not found on Character");

                return _characterController;
            }
        }
        
        #region Targetable

        [field: SerializeField] public TargetGroup TargetGroup { get; private set; }
        
        [field: SerializeField] public Targeter Targeter { get; private set; }
        
        public GameObject Obj => gameObject;

        public ITargetable.Hit TargetHit { get; set; }
        
        #endregion

        #region Damageable

        [field: SerializeField] public Vitality Vitality { get; private set; }
        
        [field: SerializeField] public Endurance Endurance { get; private set; }

        public Damager Damager { get; private set; } = new Damager();

        public virtual float LoadCurrentHealth()
        {
            return Vitality.FullValue;
        }
        
        #endregion
        
        protected virtual void Start()
        {
            if (GameManager.Instance.IsReady)
            {
                Initialize();
            }

            else
            {
                EventBus<GameManagerReady>.Subscribe(Initialize);
            }
        }

        protected virtual void Initialize()
        {
            _controllers = GetComponentsInChildren<Controller>();

            foreach (Controller controller in _controllers)
            {
                controller.Initialize(this);
                
                controller.InvokeReady();
            }
            
            this.InitializeTargetable();

            this.InitializeDamageable();
            
            InvokeReady();
        }

        public bool GetController<T>(out T controller) where T : Controller
        {
            controller = null;

            controller = (T) _controllers.FirstOrDefault(c => c is T);
        
            return controller != null;
        }
    
        #region Animation Events

        public void Equipped(int slot)
        {
            InvokeEventDispatched(nameof(Equipped), slot);
        }
        
        public void UnEquipped(int slot)
        {
            InvokeEventDispatched(nameof(UnEquipped), slot);
        }
        
        public void Equipped()
        {
            InvokeEventDispatched(nameof(Equipped), 0);
        }
        
        public void UnEquipped()
        {
            InvokeEventDispatched(nameof(UnEquipped), 0);
        }
        
        #endregion
    }
}
