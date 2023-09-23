using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core.Utils
{
    /// <summary>
    /// list of tags as a check list, uses TagMaskPropertyDrawer
    /// </summary>
    [Serializable]
    public struct TagMask
    {
        [field: SerializeField] public string[] SelectedTags { get; private set; }

        public bool Contains(string tag)
        {
            return SelectedTags.Contains(tag);
        }
    }
}
