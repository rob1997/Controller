using Character.Damage;
using TMPro;
using Ui.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Ui
{
    //uiMenu in the top region of the hud
    //contains heath bar, stamina...
    public class HudTopBarUiMenu : UiMenu
    {
        [SerializeField] private Image _healthBarImage;
        
        [SerializeField] private Image _staminaBarImage;

        private Character.Player _player;
        
        public override void Initialize(UiRegion rootUiElement)
        {
            base.Initialize(rootUiElement);

            _player = Character.Player.Instance;
            
            //initialize heath bar
            if (_player.IsReady)
            {
                InitializeStats();
            }

            else
            {
                _player.OnReady += InitializeStats;
            }
        }

        private void InitializeStats()
        {
            Vitality vitality = _player.Vitality;
            
            Endurance endurance = _player.Endurance;
            
            //update stats on initialize
            HealthUpdated();
            
            StaminaUpdated();
            
            vitality.OnValueChanged += delegate { HealthUpdated(); };
            
            void HealthUpdated()
            {
                _healthBarImage.fillAmount = vitality.NormalizedValue;
            }
            
            void StaminaUpdated()
            {
                _staminaBarImage.fillAmount = endurance.NormalizedValue;
            }
        }
    }
}
