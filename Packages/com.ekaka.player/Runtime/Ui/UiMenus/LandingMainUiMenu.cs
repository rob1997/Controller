using System.Collections;
using System.Collections.Generic;
using Core.Game;
using Core.Utils;
using Ui.Main;
using UnityEngine;
using UnityEngine.UI;

namespace Player.Ui
{
    //landing uiMenu or what you see first after load
    public class LandingMainUiMenu : UiMenu
    {
        [SerializeField] private Button _newGameButton;
        
        [SerializeField] private Button _exitGameButton;

        public override void Initialize(UiRegion rootUiElement)
        {
            base.Initialize(rootUiElement);
            //start new game
            _newGameButton.onClick.AddListener(NewGame);
            //try exit game
            _exitGameButton.onClick.AddListener(TryExit);
        }

        //start new game
        private void NewGame()
        {
            GameManager.Instance.StartGame(false);
        }

        private void TryExit()
        {
            //call cancel action on uiRoot
            //most probably loads a uiModal, are you sure...
            UiRoot.CancelAction();
        }
    }
}
