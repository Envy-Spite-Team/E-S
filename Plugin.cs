using BepInEx;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using HarmonyLib;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using Logic;
using System.Reflection;
using System.IO;
using System.Net.Http;
using TMPro;

namespace DoomahLevelLoader
{
    [BepInPlugin("doomahreal.ultrakill.levelloader", "DoomahLevelLoader", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private AssetBundle terminal;
		public static bool IsCustomLevel = false;
        private static Plugin _instance;

        public static Plugin Instance => _instance;

        public static string getConfigPath()
        {
            return Path.Combine(Paths.ConfigPath + Path.DirectorySeparatorChar + "EnvyLevels");
        }

        public static GameObject FindObjectEvenIfDisabled(string rootName, string objPath = null, int childNum = 0, bool useChildNum = false)
        {
            GameObject obj = null;
            GameObject[] objs = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
            bool gotRoot = false;
            foreach (GameObject obj1 in objs)
            {
                if (obj1.name == rootName)
                {
                    obj = obj1;
                    gotRoot = true;
                }
            }
            if (!gotRoot)
                goto returnObject;
            else
            {
                GameObject obj2 = obj;
                if (objPath != null)
                {
                    obj2 = obj.transform.Find(objPath).gameObject;
                    if (!useChildNum)
                    {
                        obj = obj2;
                    }
                }
                if (useChildNum)
                {
                    GameObject obj3 = obj2.transform.GetChild(childNum).gameObject;
                    obj = obj3;
                }
            }
        returnObject:
            return obj;
        }

        private void Awake()
        {
            Logger.LogInfo("If you see this, dont panick! because everything is fine :)");
            terminal = Loader.LoadTerminal();
			
            _instance = this;

            Harmony val = new Harmony("doomahreal.ultrakill.levelloader");
            val.PatchAll();

            if(!Directory.Exists(getConfigPath()))
            {
                Directory.CreateDirectory(getConfigPath());
            }
			
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
			Loaderscene.LoadLevels();
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
		
		private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
		{
			bool isNotBootstrapOrIntro = SceneHelper.CurrentScene != "Bootstrap" && SceneHelper.CurrentScene != "Intro";
			bool isMainMenu = SceneHelper.CurrentScene == "Main Menu";

			if (isNotBootstrapOrIntro)
			{
				ShaderManager.CreateShaderDictionary();
				InstantiateEnvyScreen(isMainMenu);
			}

			if (ShaderManager.shaderDictionary.Count <= 0)
			{
				StartCoroutine(ShaderManager.LoadShadersAsync());
			}

			if (Loaderscene.IsSceneInAnyAssetBundle(scene.name))
			{
				Fixorsmth();
			}
			else
			{
				IsCustomLevel = false;
				Loaderscene.currentLevelName = null;
			}
		}
		
		public static void Fixorsmth()
		{
			SceneHelper.CurrentScene = SceneManager.GetActiveScene().name;
			Camera mainCamera = Camera.main;
			IsCustomLevel = true;
			mainCamera.clearFlags = CameraClearFlags.Skybox;
			_instance.StartCoroutine(ShaderManager.ApplyShadersAsyncContinuously());
		}

        private void OnSceneUnloaded(Scene scene)
        {
			if (SceneHelper.CurrentScene == "Main Menu")
			{
				InstantiateEnvyScreen(true);
                ShaderManager.CreateShaderDictionary();
            }
        }

        private void InstantiateEnvyScreen(bool mainMenu)
		{
            GameObject envyScreenPrefab = terminal.LoadAsset<GameObject>("EnvyScreen.prefab");
            // Fun Fact: my dumbass forgot to put envyscreen in the assetbundle and i was stuck debugging it for 2 hours RAHHHHHHHHHHHHH --thebluenebula
			// smart ass --doomah
            if (envyScreenPrefab == null)
			{
				Debug.LogError("EnvyScreen prefab not found in the terminal bundle.");
				return;
			}

            GameObject canvasObject = GameObject.Find("/Canvas/Main Menu (1)");
            if (mainMenu == false)
            {
                canvasObject = FindObjectEvenIfDisabled("Canvas", "PauseMenu");
            }

            if (canvasObject == null)
			{
                return;
			}

			GameObject instantiatedObject = Instantiate(envyScreenPrefab);

            instantiatedObject.transform.SetParent(canvasObject.transform, false);
			instantiatedObject.transform.localPosition = Vector3.zero;
			instantiatedObject.transform.localScale = new Vector3(1f, 1f, 1f);
		}
    }
}
