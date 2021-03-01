using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Controller : MonoBehaviour
{
    private List<Action> _actions = new List<Action>();

    private Character _character;
    
    public virtual void Initialize(Character character)
    {
        _character = character;
    }

    public void AddActions(List<Action> actions)
    {
        actions.ForEach(action =>
        {
            if (_actions.Find(a => a.GetType() == action.GetType()) == null)
            {
                _actions.Add(action);
            
                action.Initialize(this);
            }

            else
            {
                Debug.LogError($"action {action} already exists");
            }
        });
    }
    
    public void TakeAction<T>() where T : Action
    {
        if (GetAction(out T action))
        {
            action.OnAction();
        }
    }
    
    public bool GetAction<T>(out T action) where T : Action
    {
        action = null;

        action = (T) _actions.Find(a => a is T);

        if (action == null)
        {
            Debug.LogError($"can't find action {typeof(T)}");
        }
        
        return action != null;
    }
    
    public Character GetCharacter()
    {
        return _character;
    }
}
