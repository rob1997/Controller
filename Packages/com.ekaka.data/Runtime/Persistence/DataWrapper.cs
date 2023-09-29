using System;
using System.IO;
using Core.Utils;
using Data.Main;
using Newtonsoft.Json;
using UnityEngine;

namespace Data.Persistence
{
    [Serializable]
    public class DataWrapper<TDataModel> : IDataWrapper where  TDataModel : IDataModel, new()
    {
        [field: SerializeField] public string Id { get; private set; }
        
        [field: NonSerialized] public bool Initialized { get; private set; }

        //use GetFullPath to return machine specific/independent path
        //path separators are different for different platforms and OS
        public string DataPath => Path.GetFullPath($"{Application.persistentDataPath}/{typeof(TDataModel).Name}/{Id}.json");

        public bool FileExists => File.Exists(DataPath);
        
        [field: SerializeField] public TDataModel DataModel { get; private set; }
        
        public bool SaveData()
        {
            if (!FileExists)
            {
                Debug.Log($"creating file at {DataPath}...");

                string directory = Path.GetDirectoryName(DataPath);
                //create directory if it doesn't exist
                if (!Directory.Exists(directory))
                {
                    if (!string.IsNullOrEmpty(directory))
                    {
                        Directory.CreateDirectory(directory);
                    }

                    else
                    {
                        Debug.LogError($"no directory found at {DataPath}");
                        
                        return false;
                    }
                }
            }

            try
            {
                File.WriteAllText(DataPath, JsonConvert.SerializeObject(DataModel, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                }));
                
                Debug.Log($"file saved at {DataPath}");

                return true;
            }
            
            catch (Exception e)
            {
                e.LogToUnity();
                
                return false;
            }
        }

        public bool LoadData()
        {
            if (!FileExists)
            {
                Debug.LogError($"can't load file at {DataPath}, doesn't exist");

                return false;
            }

            try
            {
                string jsonText = File.ReadAllText(DataPath);
                //assign DataModel
                DataModel = JsonConvert.DeserializeObject<TDataModel>(jsonText, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                });

                //set initialized to true
                if (!Initialized)
                {
                    Initialized = true;
                }
                
                return true;
            }
            
            catch (Exception e)
            {
                e.LogToUnity();

                return false;
            }
        }

        public bool ResetData(bool newFile = true)
        {
            if (!string.IsNullOrEmpty(Id))
            {
                if (FileExists)
                {
                    Debug.Log($"removing file at {DataPath}...");

                    try
                    {
                        File.Delete(DataPath);
                    }
                    
                    catch (Exception e)
                    {
                        e.LogToUnity();
                        
                        return false;
                    }
                }

                else
                {
                    Debug.LogWarning($"can't remove file at {DataPath}, file doesn't exist");
                }
            }

            if (newFile)
                Id = Guid.NewGuid().ToString();
            
            DataModel = new TDataModel();
            
            return SaveData();
        }
    }
}
