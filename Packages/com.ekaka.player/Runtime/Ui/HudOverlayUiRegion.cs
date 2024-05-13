using System;
using System.Collections;
using System.Collections.Generic;
using Core.Game;
using Core.Common;
using Ui.Main;
using UnityEngine;

namespace Player.Ui
{
    public class HudOverlayUiRegion : UiRegion
    {
        [SerializeField] private UiModalData _pauseUiModal;

        //this should have ForcePick enabled/player must pick an option
        [SerializeField] private UiModalData _gameOverUiModal;

        //if playing then this uiRegion is active
        //can trigger cancel action even with no activeUiMenu
        public override bool IsActive => GameManager.Instance.CurrentState is Play;

        public override void Initialize(UiLayer rootUiElement)
        {
            base.Initialize(rootUiElement);

            OnUiMenuStateChanged += uiMenu =>
            {
                switch (uiMenu.UiMenuState)
                {
                    case UiMenuState.Loading:
                        //only pause from play
                        if (GameManager.Instance.CurrentState is Play)
                        {
                            PauseGame();
                        }
                        break;
                    
                    case UiMenuState.Unloaded:
                        //only resume from pause
                        if (GameManager.Instance.CurrentState is Pause)
                        {
                            ResumeGame();
                        }
                        break;
                }
            };
            
            EventBus<GameStateEnabled<GameOver>>.Subscribe(GameOver);
        }

        private void PauseGame()
        {
            GameManager.Instance.PauseGame();
        }
        
        private void ResumeGame()
        {
            GameManager.Instance.ResumeGame();
        }

        private void GameOver()
        {
            //queue/load uiModal
            UiRoot.QueueUiModal(_gameOverUiModal);
        }
        
        //override cancel action with an exit uiModal
        public override void CancelAction()
        {
            //queue/load uiModal
            UiRoot.QueueUiModal(_pauseUiModal);
        }
    }
}
