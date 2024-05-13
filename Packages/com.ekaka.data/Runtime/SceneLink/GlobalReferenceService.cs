using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Data.SceneLink
{
    public class GlobalReferenceService
    {
        #region ReferenceLoaded

        public delegate void ReferenceLoaded();

        public event ReferenceLoaded OnReferenceLoaded;

        private void InvokeReferenceLoaded()
        {
            OnReferenceLoaded?.Invoke();
        }

        #endregion
    
        /// <summary>
        /// array of all loaded global references
        /// loaded as in in any active scene or in project
        /// </summary>
        public GlobalReference[] AllLoadedReferences { get; private set; } = { };

        public void AddReferences(GlobalReference[] references)
        {
            int length = references.Length;
        
            references = references.Where(r => AllLoadedReferences.All(aR => aR.Id != r.Id)).ToArray();

            int diff = length - references.Length;
            //highly unlikely, only occurs if Guid is duplicate, chances are astronomically low
            if (diff > 0)
            {
                Debug.LogWarning($"failed adding {diff} {nameof(GlobalReference.Reference)} due to duplicate Id, please regenerate");
            }
        
            AllLoadedReferences = AllLoadedReferences.Concat(references).ToArray();
        
            InvokeReferenceLoaded();
        }
    
        public void RemoveReference(string[] referenceIds)
        {
            int length = AllLoadedReferences.Length;

            referenceIds = referenceIds.Where(id => AllLoadedReferences.Any(r => r.Id == id)).ToArray();

            int diff = length - referenceIds.Length;
            //this only happens if id isn't found in AllReferences
            if (diff > 0)
            {
                Debug.LogWarning($"failed removing {diff} {nameof(GlobalReference.Reference)}, {nameof(GlobalReference.Id)} not found");
            }

            AllLoadedReferences = AllLoadedReferences.Where(r => !referenceIds.Contains(r.Id)).ToArray();
        }

        public Component GetComponentReference(string id)
        {
            return AllLoadedReferences.SingleOrDefault(r => r.Id == id).Reference;
        }
    }
}
