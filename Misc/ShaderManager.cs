using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using EnvyLevelLoader.Loaders;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;
using UnityEngine.SceneManagement;

namespace EnvyLevelLoader
{
    public static class ShaderManager
    {
        public static Dictionary<string, Shader> shaderDictionary = new Dictionary<string, Shader>();
        private static HashSet<Material> modifiedMaterials = new HashSet<Material>();

        public static string ModPath()
        {
            return Assembly.GetExecutingAssembly().Location.Substring(0, Assembly.GetExecutingAssembly().Location.LastIndexOf(Path.DirectorySeparatorChar));
        }

        public static IEnumerator ApplyShadersAsync(GameObject[] allGameObjects)
        {
            if (allGameObjects == null)
            {
                yield break;
            }

            foreach (GameObject go in allGameObjects)
            {
                if (go == null)
                    continue;

                Renderer[] meshRenderers = go.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer renderer in meshRenderers)
                {
                    if (renderer == null)
                        continue;

                    Material[] newMaterials = new Material[renderer.sharedMaterials.Length];
                    for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                    {
                        Material sharedMat = renderer.sharedMaterials[i];
                        newMaterials[i] = sharedMat;

                        if (sharedMat == null || sharedMat.shader == null || modifiedMaterials.Contains(sharedMat))
                            continue;

                        if (sharedMat.shader.name == "ULTRAKILL/PostProcessV2")
                            continue;

                        if (!shaderDictionary.TryGetValue(sharedMat.shader.name, out Shader realShader))
                        {
                            continue;
                        }

                        Debugger.Log($"Swapping shader {newMaterials[i].shader} with {realShader.name}");
                        newMaterials[i].shader = realShader;
                        modifiedMaterials.Add(sharedMat);
                    }
                    renderer.materials = newMaterials;
                }
                yield return null;
            }
        }

        public static IEnumerator ApplyShadersAsyncContinuously()
        {
            GameObject shaderManagerObject = new GameObject("ShaderManagerObject");
            ShaderManagerRunner shaderManagerRunner = shaderManagerObject.AddComponent<ShaderManagerRunner>();

            shaderManagerRunner.StartApplyingShaders();
            yield return null;
        }

        public static void CreateShaderDictionary()
        {
            Debugger.Log("building shader dictionary");
            shaderDictionary = new Dictionary<string, Shader>();

            List<string> allShaders = new List<string>();
            foreach (IResourceLocator resourceLocator in Addressables.ResourceLocators)
            {
                foreach (object obj in resourceLocator.Keys)
                {
                    IList<IResourceLocation> list;
                    bool flag = resourceLocator.Locate(obj, typeof(object), out list);
                    if (flag)
                    {
                        foreach (IResourceLocation resourceLocation in list)
                        {
                            if(Path.GetExtension(resourceLocation.PrimaryKey) != ".shader")
                                continue;
                            Debugger.Log("Loading " + resourceLocation.PrimaryKey + " as shader");
                            allShaders.Add(resourceLocation.PrimaryKey);
                        }
                    }
                }
            }

            Debugger.Log("Loading shaders please wait...");
            new GameObject("tmp shader loader").AddComponent<LevelLoader.Dummy>().StartCoroutine(LoadShadersAsync(allShaders));
        }

        static IEnumerator LoadShadersAsync(List<string> allShaders)
        {
            foreach (var shaderKey in allShaders)
            {
                var req = Addressables.LoadAssetAsync<Shader>(shaderKey);
                yield return req;
                if (req.Status != AsyncOperationStatus.Succeeded)
                {
                    Debugger.LogError("Failed to load shader: " + shaderKey);
                    continue;
                }
                Shader s = req.Result;
                shaderDictionary[s.name] = s;
                Debugger.Log("Got shader " + s.name);
            }

            Debugger.Log("Loaded shaders!");
        }
    }

    public class ShaderManagerRunner : MonoBehaviour
    {
        public void StartApplyingShaders()
        {
            StartCoroutine(ApplyShadersContinuously());
        }

        private IEnumerator ApplyShadersContinuously()
        {
            yield return ShaderManager.ApplyShadersAsync(SceneManager.GetActiveScene().GetRootGameObjects());

            while (true)
            {
                yield return new WaitForFixedUpdate();

                yield return ShaderManager.ApplyShadersAsync(SceneManager.GetActiveScene().GetRootGameObjects());
            }
        }

        // Get inspector references (e.g., serialized fields in MonoBehaviour components)
        GameObject[] GetInspectorReferencedObjects()
        {
            List<GameObject> objs = new List<GameObject>();
            var components = FindObjectsOfTypeAll(typeof(MonoBehaviour)); // Find all MonoBehaviours

            foreach (var component in components)
            {
                // Use reflection to get all serialized fields (even private ones)
                var fields = component.GetType().GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                foreach (var field in fields)
                {
                    // Check if the field is marked as serialized
                    if (field.IsPublic || field.GetCustomAttributes(typeof(SerializeField), false).Length > 0)
                    {
                        object value = field.GetValue(component);

                        if (value != null && value is GameObject)
                        {
                            objs.Add((GameObject)value);
                        }
                    }
                }
            }
            return objs.ToArray();
        }
    }
}
