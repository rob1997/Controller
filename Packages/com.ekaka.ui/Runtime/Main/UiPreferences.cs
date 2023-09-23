using System.Collections;
using System.Collections.Generic;
using Core.Game;
using Core.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ui.Main
{
    //save all uiPreference data/data that's related to appearance
    [CreateAssetMenu(fileName = nameof(UiPreferences), menuName = GameManager.StudioPrefix + "/Ui/Preference", order = 0)]
    public class UiPreferences : ScriptableObject
    {
        [field: SerializeField] public float MenuCacheTimeOut { get; private set; } = 5f;
    
        [field: SerializeField] public UiTransition DefaultUiTransition { get; private set; }
        
        [field: SerializeField] public RectOffset DefaultUiModalPadding { get; private set; }
    }
}
