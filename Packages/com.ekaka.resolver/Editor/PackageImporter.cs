using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace GitResolver
{
    [InitializeOnLoad]

    public static class PackageImporter
    {
        private const string ManifestPath = "./Packages/manifest.json";

        static PackageImporter()
        {
            EditorApplication.projectChanged += Resolve;

            Resolve();
        }

        private static void Resolve()
        {
            ListRequest pkgListRequest = Client.List();

            while (!pkgListRequest.IsCompleted)
            {
                //do nothing
            }

            bool resolvePkg = false;

            ProjectManifest manifest = JsonConvert.DeserializeObject<ProjectManifest>(File.ReadAllText(ManifestPath));

            foreach (PackageInfo pkg in pkgListRequest.Result)
            {
                //only for custom pkgs
                if (!pkg.name.StartsWith("com.ekaka"))
                {
                    continue;
                }

                var package =
                    JsonConvert.DeserializeObject<GitPackage>(File.ReadAllText($"{pkg.resolvedPath}/Package.json"));

                if (package.GitDependencies != null && package.GitDependencies.Count > 0)
                {
                    //reslove
                    foreach (var pair in package.GitDependencies)
                    {
                        //check if already resolved
                        if (!manifest.Dependencies.ContainsKey(pair.Key))
                        {
                            resolvePkg = true;

                            manifest.Dependencies.Add(pair.Key, pair.Value);
                        }
                    }
                }
            }

            if (resolvePkg)
            {
                AssetDatabase.StartAssetEditing();

                File.WriteAllText("./Packages/manifest.json", JsonConvert.SerializeObject(manifest));

                AssetDatabase.StopAssetEditing();
            }
        }

        public struct GitPackage
        {
            [JsonProperty("gitDependencies")] public Dictionary<string, string> GitDependencies { get; private set; }
        }

        [Serializable]
        public struct ProjectManifest
        {
            [JsonProperty("dependencies")] public Dictionary<string, string> Dependencies { get; private set; }
        }
    }
}
