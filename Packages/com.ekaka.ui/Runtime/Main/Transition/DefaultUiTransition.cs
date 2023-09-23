using System;
using System.Collections;
using System.Collections.Generic;
using Core.Game;
using Ui.Main;
using UnityEngine;

namespace Ui.Main
{
    [CreateAssetMenu(fileName = nameof(DefaultUiTransition), menuName = GameManager.StudioPrefix + "/Ui/Transitions/Default", order = 0)]
    public class DefaultUiTransition : UiTransition
    {
        public override void Setup(UiMenu uiMenu)
        {
            uiMenu.gameObject.SetActive(false);
        }

        public override void Load(UiMenu uiMenu, Action onComplete = null)
        {
            uiMenu.gameObject.SetActive(true);
        
            onComplete?.Invoke();
        }

        public override void Unload(UiMenu uiMenu, Action onComplete = null)
        {
            uiMenu.gameObject.SetActive(false);

            onComplete?.Invoke();
        }
    }
}
