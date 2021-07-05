using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : Manager
{
    
    #region AssetEnabled

    public delegate void AssetEnabled();

    public event AssetEnabled OnAssetEnabled;

    private void InvokeAssetEnabled()
    {
        OnAssetEnabled?.Invoke();
    }

    #endregion

    #region AssetDisabled

    public delegate void AssetDisabled();

    public event AssetDisabled OnAssetDisabled;

    private void InvokeAssetDisabled()
    {
        OnAssetDisabled?.Invoke();
    }

    #endregion
    
    public PlayerInputActions InputActions { get; private set; }

    public override void Initialize()
    {
        InputActions = new PlayerInputActions();
        
        InputActions.Enable();
    }

    public void EnableFoot()
    {
        InputActions.Foot.Enable();
    }
    
    public void DisableFoot()
    {
        InputActions.Foot.Disable();
    }
    
    public void EnableAsset()
    {
        InputActions.asset.Enable();
        
        InvokeAssetEnabled();
    }
    
    public void DisableAsset()
    {
        InputActions.asset.Disable();
        
        InvokeAssetDisabled();
    }
}
