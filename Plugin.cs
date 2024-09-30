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
using Steamworks;
using UnityEngine.UI;

namespace DoomahLevelLoader
{
    [BepInPlugin("doomahreal.ultrakill.levelloader", "DoomahLevelLoader", "2.0.0")] // Make sure to change after each update!
    public class Plugin : BaseUnityPlugin
    {
        private AssetBundle terminal;
        private AssetBundle envydl_debug;
        public static bool IsCustomLevel = false;
        private static Plugin _instance;

        public GameObject instantiatedDebug;

        public string Version = "2.0.0"; // Make sure to change after each update!

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

        private async void Awake()
        {
            Logger.LogInfo("If you see this, dont panick! because everything is fine :)");
            terminal = Loader.LoadTerminal();

            _instance = this;

            Harmony val = new Harmony("doomahreal.ultrakill.levelloader");
            val.PatchAll();

            if (!Directory.Exists(getConfigPath()))
            {
                Directory.CreateDirectory(getConfigPath());
            }

            await Merger.MergeFiles();

            // After merging, load the levels
            Loaderscene.LoadLevels();

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void Update() //this should fix the envy button not appearing sometimes --triggered
        {
            if(!hasMadeEnvyScreen)
            {
                bool isNotBootstrapOrIntro = SceneHelper.CurrentScene != "Bootstrap" && SceneHelper.CurrentScene != "Intro";
                bool isMainMenu = SceneHelper.CurrentScene == "Main Menu";

                if (isNotBootstrapOrIntro)
                    InstantiateEnvyScreen(isMainMenu);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        static bool hasMadeEnvyScreen = false;
        static ShowDebugInfo ShowDebugInfoInstance = new ShowDebugInfo();
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            hasMadeEnvyScreen = false;
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

            // this was broken af with the new code
            if (!Loaderscene.IsSceneInAnyAssetBundle(scene.name))
            {
                IsCustomLevel = false;
                Loaderscene.currentLevelName = null;
            }
            else IsCustomLevel = true;

            // Register cheats
            if (Plugin.IsCustomLevel)
            {
                GameObject debugInfoPrefab = terminal.LoadAsset<GameObject>("DebugInfo.prefab");
                instantiatedDebug = Instantiate(debugInfoPrefab);

                instantiatedDebug.transform.SetParent(GameObject.Find("/Canvas").transform, false);
                instantiatedDebug.SetActive(false);
                instantiatedDebug.transform.localPosition = new Vector3(-699, 60, 0);
                instantiatedDebug.transform.localScale = new Vector3(2, 2, 2);

                CheatsManager.Instance.RegisterCheat(ShowDebugInfoInstance, "Envy");
                GameObject pt = GameObject.Find("/Canvas/Main Menu (1)/EnvyScreen(Clone)/PlayTab");
                Debugger.Log($"ASJIKPOGJKDGJKDJ {pt}");
                if (pt != null) {
                    pt.transform.localScale *= 1.25f;
                }
            }
            else
            {
                try
                {
                    CheatsManager.Instance.allRegisteredCheats["Envy"].Remove(ShowDebugInfoInstance);
                }
                catch { }
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
            // i find it funny how your laptop broke right after saying that --thebluenebula
            if (envyScreenPrefab == null)
            {
                Debugger.LogError("EnvyScreen prefab not found in the terminal bundle.");
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
            Transform play = instantiatedObject.transform.Find("PlayTab");
            Transform brows = instantiatedObject.transform.Find("BrowseTab");
            play.localScale *= 1.35f;
            brows.localScale *= 1.35f;

            hasMadeEnvyScreen = true;

            if (SteamClient.SteamId.Value == 76561198275729385 || SteamClient.SteamId.Value == 76561199017586561) //doomah what the fuck is your stea- oh yeah... --triggered
            {
                if(envydl_debug == null)
                    envydl_debug = Loader.LoadEnvyDLDev();
                GameObject menu = envydl_debug.LoadAsset<GameObject>("envy_dl_dev_menu.prefab");
                GameObject menu_btn = envydl_debug.LoadAsset<GameObject>("envy_dl_debug.prefab");
                if (menu == null)
                { Debugger.LogError("envy_dl_dev_menu is null!"); return; }
                if (menu_btn == null)
                { Debugger.LogError("envy_dl_debug is null!"); return; }

                menu = Instantiate(menu);
                menu.transform.SetParent(canvasObject.transform, false);
                menu.SetActive(false);

                menu_btn = Instantiate(menu_btn);
                menu_btn.transform.SetParent(canvasObject.transform, false);
                menu_btn.SetActive(true);

                menu_btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    menu.SetActive(!menu.activeSelf);
                });

                menu.AddComponent<EnvyDownloaderMenu_DEBUG>().menuButton = menu_btn;
            }
        }
    }
}