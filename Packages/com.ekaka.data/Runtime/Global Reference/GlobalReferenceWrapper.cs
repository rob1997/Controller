using System;
using System.IO;
using System.Linq;
using Core.Utils;
using Data.Main;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.GlobalReference
{
    [Serializable]
    public struct GlobalReference : IDataModel
    {
        [field: SerializeField] public string Id { get; private set; }

        [JsonIgnore] public Type Type => Reference != null ? Reference.GetType() : Type.GetType(TypeName);

        [field: SerializeField] public string TypeName { get; private set; }

        /// <summary>
        /// IMPORTANT! do not change value during runtime, since structs are value types changes will not be reflected on GlobalReferenceService (it's a copy)
        /// </summary>
        [JsonIgnore]
        [field: SerializeField]
        public Component Reference { get; private set; }

        public GlobalReference(string id, Component reference, string typeName = default)
        {
            Id = id;

            Reference = reference;

            TypeName = Reference != null ? Reference.GetType().AssemblyQualifiedName : typeName;
        }

        public void SetReference(Component reference)
        {
            if (reference == null)
            {
                Debug.LogWarning($"{nameof(Reference)} can't be null or empty");

                return;
            }

            Reference = reference;
        }
    }

    public class GlobalReferenceWrapper : MonoBehaviour
    {
        [field: SerializeField] public GlobalReference[] References { get; private set; } = { };

        public static string ReferenceFilePath =>
            Path.GetFullPath($"{Application.persistentDataPath}/{nameof(GlobalReference)}.json");

        private GlobalReferenceService _referenceService;

        private void OnEnable()
        {
            if (DataManager.Instance.IsReady) Initialize();

            else DataManager.Instance.OnReady += Initialize;
        }

        private void Initialize()
        {
            _referenceService = DataManager.Instance.GlobalReferenceService;

            _referenceService.AddReferences(References);
        }

        public bool AddReference(GlobalReference reference)
        {
            //check if reference has a correct id
            if (string.IsNullOrEmpty(reference.Id))
            {
                Debug.LogWarning($"can't add {nameof(GlobalReference)}, empty or null {nameof(GlobalReference.Id)}");

                return false;
            }

            //check if reference already exists
            if (References.Any(r => r.Id == reference.Id))
            {
                Debug.LogWarning(
                    $"can't add {nameof(GlobalReference)}, {nameof(GlobalReference.Reference)} with {nameof(GlobalReference.Id)} {reference.Id} already exists");

                return false;
            }

            //check if reference is loaded/not null
            if (reference.Reference == null)
            {
                Debug.LogWarning(
                    $"can't add {nameof(GlobalReference)}, {nameof(Component)} {nameof(GlobalReference.Reference)} is null/not loaded");

                return false;
            }

            References = References.Append(reference).ToArray();

            Debug.Log($"{nameof(GlobalReference)} {reference.Reference.name} [{reference.Type}] added");

            return AddReferenceToFile(reference);
        }

        public bool RemoveReference(string id)
        {
            if (References.All(r => r.Id != id))
            {
                Debug.LogWarning(
                    $"can't remove, {nameof(GlobalReference)} with {nameof(GlobalReference.Id)} {id} doesn't exist");

                return false;
            }

            References = References.Where(r => r.Id != id).ToArray();

            Debug.Log($"{nameof(GlobalReference)} with {nameof(GlobalReference.Id)} {id} removed");

            return RemoveReferenceFromFile(id);
        }

        private bool AddReferenceToFile(GlobalReference reference)
        {
            if (!GetReferencesFromFile(out GlobalReference[] references))
            {
                return false;
            }

            references = references.Append(reference).ToArray();

            if (SaveReferencesToFile(references))
            {
                Debug.Log($"added {nameof(GlobalReference.Reference)} {reference.Id} from file {ReferenceFilePath}");

                return true;
            }

            return false;
        }

        private bool RemoveReferenceFromFile(string id)
        {
            if (!GetReferencesFromFile(out GlobalReference[] references))
            {
                return false;
            }

            references = references.Where(r => r.Id != id).ToArray();

            if (SaveReferencesToFile(references))
            {
                Debug.Log($"removed {nameof(GlobalReference.Reference)} {id} from file {ReferenceFilePath}");

                return true;
            }

            return false;
        }

        private bool GetReferencesFromFile(out GlobalReference[] references)
        {
            references = new GlobalReference[0];

            if (!File.Exists(ReferenceFilePath))
            {
                return CreateFile();
            }
            //read file and deserialize
            else
            {
                try
                {
                    string rawJson = File.ReadAllText(ReferenceFilePath);

                    references = JsonConvert.DeserializeObject<GlobalReference[]>(rawJson);

                    return true;
                }

                catch (Exception e)
                {
                    e.LogToUnity();

                    return false;
                }
            }
        }

        private bool SaveReferencesToFile(GlobalReference[] references)
        {
            if (!File.Exists(ReferenceFilePath))
            {
                if (!CreateFile())
                {
                    return false;
                }
            }

            try
            {
                string rawJson = JsonConvert.SerializeObject(references);

                File.WriteAllText(ReferenceFilePath, rawJson);

                return true;
            }

            catch (Exception e)
            {
                e.LogToUnity();

                return false;
            }
        }

        public static bool CreateFile()
        {
            try
            {
                Debug.Log($"{nameof(GlobalReference)} file doesn't exist creating...");

                string directory = Path.GetDirectoryName(ReferenceFilePath);

                //create directory if it doesn't exist
                if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(ReferenceFilePath, JsonConvert.SerializeObject(new GlobalReference[0]));

                return true;
            }

            catch (Exception e)
            {
                e.LogToUnity();

                return false;
            }
        }

        private void OnDestroy()
        {
            if (_referenceService != null)
            {
                string[] referenceIds = Array.ConvertAll(References, r => r.Id);

                _referenceService.RemoveReference(referenceIds);
            }
        }
    }
}