using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Persistence
{
    public interface IDataWrapper
    {
        string Id { get; }
        
        //data been loaded before
        bool Initialized { get; }
        
        string DataPath { get; }
        
        bool FileExists { get; }

        bool SaveData();
        
        bool LoadData();
        
        bool ResetData(bool newFile = true);
    }
}