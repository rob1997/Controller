#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;
using System.IO;

namespace UMotionEditor
{
    public class AssetResourcesFile : ScriptableObject
    {
        //********************************************************************************
        // Public Properties
        //********************************************************************************

        //********************************************************************************
        // Private Properties
        //********************************************************************************

        #pragma warning disable 0649 // Suppress "Field 'field' is never assigned to, and will always have its default value 'value'"
        [Serializable]
        private struct ResourceDefinition
        {
            public string Name;
            public UnityEngine.Object Reference;
        }
        #pragma warning restore 0649

        //----------------------
        // Inspector
        //----------------------
        [SerializeField]private List<ResourceDefinition> resourcesList = new List<ResourceDefinition>();
        [SerializeField]private List<ResourceDefinition> optionalResourcesList = new List<ResourceDefinition>();
        private Dictionary<string, UnityEngine.Object> resourcesDictionary = new Dictionary<string, UnityEngine.Object>();

        //----------------------
        // Internal
        //----------------------

        //********************************************************************************
        // Public Methods
        //********************************************************************************

        public static AssetResourcesFile FindAssetResourcesFile()
        {
            string[] resourceFilesGUID = AssetDatabase.FindAssets("UMotionResources t:AssetResourcesFile");

            if (resourceFilesGUID.Length > 1)
            {
                throw new UnityException("More than one resource file was found. Please remove all UMotion files and install UMotion again.");
            }
            else if (resourceFilesGUID.Length == 0)
            {
                throw new UnityException("Resource file not found. Please install UMotion again.");
            }
            else
            {
                AssetResourcesFile resourceFile = AssetDatabase.LoadAssetAtPath<AssetResourcesFile>(AssetDatabase.GUIDToAssetPath(resourceFilesGUID[0]));

                resourceFile.InitializeDictionary();

                return resourceFile;
            }
        }

        public string GetEditorDataPath()
        {
            string resourcesPath = AssetDatabase.GetAssetPath(this);

            string dataPath = Path.GetDirectoryName(resourcesPath);
            dataPath = Path.Combine(Path.GetDirectoryName(dataPath), "Data");
            
            return dataPath;
        }

        public T GetResource<T>(string name, bool required = true) where T : UnityEngine.Object
        {
            T loadedObject = null;
            UnityEngine.Object obj;
            if (resourcesDictionary.TryGetValue(name, out obj))
            {
                loadedObject = obj as T;
            }

            if (required && (loadedObject == null))
            {
                throw new Exception(string.Format("Resource \"{0}\" can not be loaded.", name));
            }
            else
            {
                return loadedObject;
            }
        }

        //********************************************************************************
        // Private Methods
        //********************************************************************************

        private void InitializeDictionary()
        {
            resourcesDictionary.Clear();
            foreach (ResourceDefinition resourceDef in resourcesList)
            {
                if (resourceDef.Reference == null)
                {
                    throw new UnityException(string.Format("Required resource \"{0}\" not found. Please reinstall UMotion.", resourceDef.Name));
                }
                else
                {
                    resourcesDictionary.Add(resourceDef.Name, resourceDef.Reference);
                }
            }
            foreach (ResourceDefinition resourceDef in optionalResourcesList)
            {
                resourcesDictionary.Add(resourceDef.Name, resourceDef.Reference);
            }
        }
    }
}
#endif