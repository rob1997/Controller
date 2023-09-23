using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Game
{
    public interface IManager
    {
        bool IsReady { get; }

        void Initialize();
    }
}
