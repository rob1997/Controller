using System;
using Core.Game;
using Data.SceneLink;
using Data.Persistence;
using UnityEngine;

namespace Data.Main
{
    public class DataManager : Manager<DataManager>
    {
        public GlobalReferenceService GlobalReferenceService { get; private set; } = new GlobalReferenceService();

        [field: SerializeField] public CrossSceneReference<IStorable>[] AllStorableReferences { get; private set; } = { };
    
        public override void Initialize()
        {
            //load any unInitialized storable
            GlobalReferenceService.OnReferenceLoaded += InitializeLoad;
        }

        private void InitializeLoad()
        {
            Debug.Log($"loading {nameof(IStorable.Data)}...");
            
            foreach (CrossSceneReference<IStorable> storableReference in AllStorableReferences)
            {
                //check if reference is loaded
                if (!storableReference.IsLoaded) continue;

                IDataWrapper wrapper = storableReference.Reference.Data;
                
                //only initialize Storable that wasn't initialized before
                if (wrapper.Initialized) continue;

                // check if file exists before loading
                if (!wrapper.FileExists)
                {
                    // create new file if it doesn't exist
                    wrapper.ResetData(false);
                    
                    return;
                }
                
                //load data from file
                wrapper.LoadData();
            }
            
            Debug.Log($"loaded {nameof(IStorable.Data)}");
        }
    
        public void Save()
        {
            Debug.Log($"saving {nameof(IStorable.Data)}...");
        
            foreach (CrossSceneReference<IStorable> storableReference in AllStorableReferences)
            {
                //check if reference is loaded
                if (!storableReference.IsLoaded) continue;
            
                IStorable storable = storableReference.Reference;
                //first update data to current data before saving
                storable.UpdateData();
                //then save
                storable.Data.SaveData();
            }
        
            Debug.Log($"saved {nameof(IStorable.Data)}");
        }
    }
}
