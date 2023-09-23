using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ui.Main
{
    public interface IUiElement
    {
        /// <summary>
        /// is uiElement loaded/active
        /// for example a uiRegion is active if it has a uiMenu loaded in it
        /// </summary>
        bool IsActive { get; }
        
        /// <summary>
        /// does uiElement react to cancel/esc/back input action
        /// </summary>
        bool IsCancelSensitive { get; }

        /// <summary>
        /// does uiElement inherit cancel action (when cancel/esc/back action is performed) from a RootUiElement
        /// </summary>
        bool InheritCancelAction { get; }

        int Depth { get; }
        
        // this is required because interfaces don't cleanup together with gameObjects
        bool IsNull { get; }

        /// <summary>
        /// called when cancel/back/esc input action is invoked
        /// </summary>
        void CancelAction();

        IUiElement GetRootUiElement();
    }
}
