using System;
using System.Collections;
using System.Collections.Generic;
using Core.Game;
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
        public override bool IsActive => GameManager.Instance.State == GameState.Play;

        public override void Initialize(UiLayer rootUiElement)
        {
            base.Initialize(rootUiElement);

            OnUiMenuStateChanged += uiMenu =>
            {
                switch (uiMenu.UiMenuState)
                {
                    case UiMenuState.Loading:
                        PauseGame();
                        break;
                    
                    case UiMenuState.Unloaded:
                        //only resume from pause
                        if (GameManager.Instance.State == GameState.Pause)
                        {
                            ResumeGame();
                        }
                        break;
                }
            };
            
            GameManager.Instance.OnGameStateChanged += state =>
            {
                if (state == GameState.GameOver)
                {
                    //queue/load uiModal
                    UiRoot.QueueUiModal(_gameOverUiModal);
                }
            };
        }

        public void PauseGame()
        {
            GameManager.Instance.PauseGame();
        }
        
        public void ResumeGame()
        {
            GameManager.Instance.ResumeGame();
        }

        //override cancel action with an exit uiModal
        public override void CancelAction()
        {
            //queue/load uiModal
            UiRoot.QueueUiModal(_pauseUiModal);
        }
    }
}
