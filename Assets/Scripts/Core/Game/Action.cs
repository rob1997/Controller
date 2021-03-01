using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Action
{
    #region ActionInitiated

    private delegate void ActionInitiated();

    private event ActionInitiated OnActionInitiated;

    protected void InvokeActionInitiated()
    {
        OnActionInitiated?.Invoke();
    }

    #endregion

    #region ActionCompleted

    private delegate void ActionCompleted();

    private event ActionCompleted OnActionCompleted;

    protected void InvokeActionCompleted()
    {
        OnActionCompleted?.Invoke();
    }

    #endregion
    
    private Character _character;

    private Controller _controller;

    public virtual void Initialize(Controller controller)
    {
        _controller = controller;

        _character = _controller.GetCharacter();

        OnActionInitiated += delegate { _character.InvokeActionInitiated(this); };
        OnActionCompleted += delegate { _character.InvokeActionCompleted(this); };
    }
    
    public abstract void OnAction();

    public Character GetCharacter()
    {
        return _character;
    }
    
    public Controller GetController()
    {
        return _controller;
    }
}
