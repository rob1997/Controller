using System.Collections.Generic;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;

namespace Data.Utils
{
    public class AddressableScriptableObject : ScriptableObject
#if UNITY_EDITOR
        , ISerializationCallbackReceiver
#endif
    {
        [field: HideInInspector] [field: SerializeField] public string AssetGuid { get; private set; }

#if UNITY_EDITOR

        public void OnBeforeSerialize()
        {
            var settings = AddressableAssetSettingsDefaultObject.Settings;
        
            if (!AssetDatabase.TryGetGUIDAndLocalFileIdentifier(this, out string guid, out long _))
            {
                Debug.LogError($"Failed getting guid for {name}");
            
                return;
            }
        
            // if not addressable add as new entry
            if (string.IsNullOrEmpty(AssetGuid))
            {
                var newEntries = new List<AddressableAssetEntry>();
        
                var entry = settings.CreateOrMoveEntry(guid, settings.DefaultGroup, readOnly: false, postEvent: false);
            
                entry.address = AssetDatabase.GUIDToAssetPath(guid);

                newEntries.Add(entry);
 
                settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, newEntries, true);

                AssetGuid = entry.address;
            
                EditorUtility.SetDirty(this);
            }

            // find addressable and assign address to GUID
            else
            {
                var entry = settings.FindAssetEntry(guid);
            
                if (entry.address != AssetGuid)
                {
                    AssetGuid = entry.address;
                
                    EditorUtility.SetDirty(this);
                }
            }
        }

        public void OnAfterDeserialize()
        {
        
        }
#endif
    }
}