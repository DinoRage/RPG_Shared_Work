//////////////////////////////////////////////////////
// Shader Packager
// Copyright (c)2021 Jason Booth
//////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif
using System.IO;
using System.Reflection;

namespace AwesomeTechnologies.ShaderPackager
{
    [ScriptedImporter(0, ShaderPackageImporter.k_FileExtension)]
    public class ShaderPackageImporter : ScriptedImporter
    {
        public const string k_FileExtension = ".VSPshaderpack";

        public override void OnImportAsset(AssetImportContext ctx)
        {
            string fileContent = File.ReadAllText(ctx.assetPath);
            var package = ObjectFactory.CreateInstance<ShaderPackage>();

            if (!string.IsNullOrEmpty(fileContent))
            {
                EditorJsonUtility.FromJsonOverwrite(fileContent, package);
            }

            if (package.entries == null)
            {
                package.entries = new List<ShaderPackage.Entry>();
            }

            package.Pack(false);

#if __BETTERSHADERS__
            if (package.betterShader == null && !string.IsNullOrEmpty(package.betterShaderPath))
            {
                ctx.DependsOnSourceAsset(package.betterShaderPath);
            }

            if (package.betterShader != null)
            {
                package.betterShaderPath = AssetDatabase.GetAssetPath(package.betterShader);
                ctx.DependsOnSourceAsset(package.betterShaderPath);
            }
#endif

            foreach (var e in package.entries)
            {
                if (e.shader != null)
                {
                    ctx.DependsOnSourceAsset(AssetDatabase.GetAssetPath(e.shader));
                }
            }

            string shaderSrc = package.GetShaderSrc();
            if (shaderSrc == null)
            {
                Debug.LogError("No Shader for this platform and SRP provided");
                // maybe make an error shader here?
                return;
            }

            Shader shader = ShaderUtil.CreateShaderAsset(ctx, shaderSrc, false);

            ctx.AddObjectToAsset("MainAsset", shader);
            ctx.SetMainObject(shader);
            
            //RefreshMaterials(ctx.assetPath);
        }
        
        void RefreshMaterials(string shaderPath)
        {
            List<string> refreshMaterialList = new List<string>();
            string[] allMaterials = AssetDatabase.FindAssets("t:Material");
            for (int i = 0; i < allMaterials.Length; i++)
            {
                string materialPath = AssetDatabase.GUIDToAssetPath(allMaterials[i]);
                string[] dep = AssetDatabase.GetDependencies(materialPath);
                for (int j = 0; j < dep.Length; j++)
                {
                    if (dep[j] == shaderPath)
                    {
                        refreshMaterialList.Add(allMaterials[i]);
                    }
                }
            }

            for (int i = 0; i < refreshMaterialList.Count; i++)
            {
                RefreshPrefabs(refreshMaterialList[i]);
            }
            
          
        }
        
        void RefreshPrefabs(string materialPath)
        {
            List<string> refreshPrefabList = new List<string>();
            string[] allPrefabs = AssetDatabase.FindAssets("t:Prefab");
            for (int i = 0; i < allPrefabs.Length; i++)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(allPrefabs[i]);

                string[] dep = AssetDatabase.GetDependencies(prefabPath, true);
                for (int j = 0; j < dep.Length; j++)
                {
                    Debug.Log("Dep: " + dep[j]);
                    if (dep[j] == materialPath)
                    {
                        refreshPrefabList.Add(allPrefabs[i]);
                    }
                }
            }

            for (int i = 0; i < refreshPrefabList.Count; i++)
            {
                AssetDatabase.ImportAsset(refreshPrefabList[i]);
            }
        }
    }
}