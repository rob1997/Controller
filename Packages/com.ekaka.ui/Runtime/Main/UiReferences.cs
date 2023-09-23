using System.Collections;
using System.Collections.Generic;
using Core.Game;
using Core.Utils;
using Ui.Utils;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Ui.Main
{
    //save all uiReference data/data that's not related to appearance
    //like ui Types (uiMenu, uiRegion, uiLayer) LandingMenus...
    [CreateAssetMenu(fileName = nameof(UiReferences), menuName = GameManager.StudioPrefix + "/Ui/Reference", order = 0)]
    public class UiReferences : ScriptableObject
    {
        [field: HideInInspector] public GenericDictionary<string, AssetReference> UiMenuReferences = new GenericDictionary<string, AssetReference>();
        
        [field: HideInInspector] public string[] UiRegionTypes = { };
        
        [field: HideInInspector] public string[] UiLayerTypes = { };
        
        [field: HideInInspector] public GenericDictionary<int, string[]> LandingUiMenus = new GenericDictionary<int, string[]>();
        
        [field: UiMenuType] [field: SerializeField] public string DefaultUiModalMenuType { get; private set; }
        
        public bool GetMenuReference(string uiMenuType, out AssetReference menuReference)
        {
            return UiMenuReferences.TryGetValue(uiMenuType, out menuReference);
        }
    }
}
