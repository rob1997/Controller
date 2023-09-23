using System;
using System.Collections;
using System.Collections.Generic;
using Ui.Main;
using UnityEngine;

namespace Ui.Main
{
    public abstract class UiTransition : ScriptableObject
    {
        public abstract void Setup(UiMenu uiMenu);
    
        public abstract void Load(UiMenu uiMenu, Action onComplete = null);
        
        public abstract void Unload(UiMenu uiMenu, Action onComplete = null);
    }
}