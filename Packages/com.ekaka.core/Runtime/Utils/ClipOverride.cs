using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Utils
{
    [Serializable]
    public class ClipOverride
    {
#if UNITY_EDITOR
        //cached values for adding new overrides
        public int selectedClipIndex;
        
        public AnimationClip selectedClip;
#endif
        
        public static string[] OverrideNames = 
        {
            //Two Hand
            "EquipTwoHand",
            "UnEquipTwoHand",
            "IdleTwoHand",
            
            //Left Hand
            "EquipLeftHand",
            "UnEquipLeftHand",
            "IdleLeftHand",
            
            //Right Hand
            "EquipRightHand",
            "UnEquipRightHand",
            "IdleRightHand",
        };

        [field: SerializeField] 
        public GenericDictionary<string, AnimationClip> Overrides 
        { get; private set; } = new GenericDictionary<string, AnimationClip>();

        public void Remove(string key)
        {
            Overrides.Remove(key);
        }
    }
}
