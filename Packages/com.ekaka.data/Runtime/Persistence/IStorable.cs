using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Data.Persistence
{
    public interface IStorable
    {
        //implement a serializable DataWrapper<T> where T : IDataModel and return that
        IDataWrapper Data { get; }
        
        //assign object data to data fields
        //this is useful before saving data
        void UpdateData();
    }
}
