using System;
using UnityEngine;


namespace AwesomeTechnologies.Utility
{
    public class MaterialUtility
    {
        public static void EnableMaterialInstancing(GameObject go)
        {
            MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                Material[] materials = meshRenderer.sharedMaterials;
                foreach (Material material in materials)
                {
                    if (!material.enableInstancing)
                    {
                        try
                        {
                            material.enableInstancing = true;
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                            throw;
                        }
                    }
                }
            }
        }

        public static void ChangeShader(GameObject go, Shader shader)
        {
            MeshRenderer[] meshRenderers = go.GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer meshRenderer in meshRenderers)
            {
                Material[] materials = new Material[meshRenderer.sharedMaterials.Length];
                for (int i = 0; i < materials.Length; i++)
                {
                    materials[i] = new Material(meshRenderer.sharedMaterials[i]);
                    materials[i].shader = shader;
                }

                meshRenderer.sharedMaterials = materials;
            }
        }

        public static void ChangeShader(Material[] materials, Shader shader)
        {
            for (int i = 0; i < materials.Length; i++)
            {
                if (materials[i]) materials[i].shader = shader;
            }
        }
    }
}