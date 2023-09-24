#if UNITY_EDITOR

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

// Initializes samples
[InitializeOnLoad]
public static class SampleInitializer
{
    // Create player data file and reference in data manager and GlobalReference

    private const string PackageName = "com.ekaka.player";

    private static string _importPath;
    
    static SampleInitializer()
    {
        string initializedKey = "SampleInitialized";
        
        // sample already initialized
        if (EditorPrefs.GetBool(initializedKey, false))
            return;
        
        Debug.Log("Initializing imported sample...");
        
        // Add game scenes to build settings
        AddScenesToBuildSettings();
        
        // Mark all UI prefabs into Addressable
        MarkUiPrefabsAddressable();
        
        // Assign urp render pipeline asset Graphics Settings
        AssignUrpGraphicsAsset();
        
        EditorPrefs.SetBool(initializedKey, true);
    }

    private static void AddScenesToBuildSettings()
    {
        var listRequest = Client.List();

        while (!listRequest.IsCompleted)
        {
            //do nothing
        }

        PackageInfo[] packages = listRequest.Result.ToArray();

        PackageInfo package = packages.FirstOrDefault(p => p.name == PackageName);

        if (package == null)
        {
            Debug.LogError($"Installed Package {PackageName} not found");

            return;
        }

        Sample sample = Sample.FindByPackage(package.name, package.version).FirstOrDefault();

        _importPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), sample.importPath);

        string scenesPath = Path.Combine(_importPath, "Scenes");

        //scenes already added to build settings
        if (EditorBuildSettings.scenes.Any(s => Path.GetFullPath(s.path).Contains(scenesPath)))
        {
            Debug.Log("Sample Scenes already initialized");
            
            return;
        }

        EditorBuildSettings.scenes = new EditorBuildSettingsScene[]
            {
                new EditorBuildSettingsScene(Path.Combine(scenesPath, "0_Loading.Unity"), true),
                new EditorBuildSettingsScene(Path.Combine(scenesPath, "1_Landing.Unity"), true),
                new EditorBuildSettingsScene(Path.Combine(scenesPath, "2_Game.Unity"), true),
            }.Concat(EditorBuildSettings.scenes)
            .ToArray();
    }

    private static void MarkUiPrefabsAddressable()
    {
        var settings = AddressableAssetSettingsDefaultObject.Settings;
        var group = settings.DefaultGroup;
        
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[]
        {
            Path.Combine(_importPath, "Prefabs", "Ui")
        });
 
        var addedEntries = new List<AddressableAssetEntry>();
        
        for (int i = 0; i < guids.Length; i++)
        {
            var entry = settings.CreateOrMoveEntry(guids[i], group, readOnly: false, postEvent: false);
            entry.address = AssetDatabase.GUIDToAssetPath(guids[i]);

            addedEntries.Add(entry);
        }
 
        settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryMoved, addedEntries, true);
    }

    private static void AssignUrpGraphicsAsset()
    {
        var universalRenderPipelineAsset = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Path.Combine(_importPath, "Resources", "UniversalRenderPipelineAsset.asset"));

        GraphicsSettings.renderPipelineAsset = universalRenderPipelineAsset;
    }
}

#endif