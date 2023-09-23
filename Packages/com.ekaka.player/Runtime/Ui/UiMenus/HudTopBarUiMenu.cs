using System.Collections;
using System.Collections.Generic;
using Damage;
using Damage.Main;
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
        
        [SerializeField] private TextMeshProUGUI _healthLabel;

        private Character.Player _player;
        
        public override void Initialize(UiRegion rootUiElement)
        {
            base.Initialize(rootUiElement);

            _player = Character.Player.Instance;
            
            //initialize heath bar
            if (_player.IsReady)
            {
                InitializeHealthBar();
            }

            else
            {
                _player.OnReady += InitializeHealthBar;
            }
        }

        private void InitializeHealthBar()
        {
            Vitality vitality = _player.Vitality;
            
            void HealthUpdated()
            {
                _healthBarImage.fillAmount = vitality.NormalizedHealth;
            
                _healthLabel.text = $"{vitality.CurrentHealth}";
            }
            //update health on initialize
            HealthUpdated();
            
            vitality.OnDamageTaken += damage => { HealthUpdated(); };
            
            vitality.OnHeathGained += damage => { HealthUpdated(); };
        }
    }
}
