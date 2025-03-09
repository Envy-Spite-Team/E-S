using System;
using BepInEx;
using EnvyLevelLoader.Loaders;
using EnvyLevelLoader.UI;
using HarmonyLib;
using Steamworks;
using Steamworks.Data;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Logger = BepInEx.Logging.Logger;
using Object = UnityEngine.Object;

namespace EnvyLevelLoader
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "envyandspite.ultrakill.envylevelloader";
        private const string modName = "envylevelloader";
        private const string modVersion = "1.8.2";

        private static readonly Harmony Harmony = new Harmony(modGUID);

        public static AssetBundle menu;
        public static GameObject menuPrefab;
        public static GameObject iconPrefab;
        public static GameObject canvasForEnvy;
        public static GameObject currentMenuInstance;
        public static GameObject currentIconInstance;

        public static Plugin Instance { get; private set; }
        
        internal static ManualLogSource PluginLogger => Instance.Logger;

        private void Awake()
        {
            Instance = this;
            Debug.Log("Loading envy level loader...");
            SceneManager.sceneLoaded += (Scene s, LoadSceneMode lcm) =>
            {
                EnvyUtility.CaptureMainThread();
                if (menu == null)
                {
                    menu = ResourceLoader.GetBundle("envymenu");
            
                    Debugger.Log("Testing bundle integrity...");
                    Object[] bundleObjects = Plugin.menu.LoadAllAssets();
                    foreach (Object obj in bundleObjects)
                    {
                        Debugger.Log($"Found {obj.name} [{obj.GetType().FullName}] in bundle {menu.name}.");
                    }
                    Debugger.Log("____________________");
            
                    menuPrefab = menu.LoadAsset<GameObject>("EnvyMenu");
                    iconPrefab = menu.LoadAsset<GameObject>("EnvyIcon");
                    canvasForEnvy = menu.LoadAsset<GameObject>("CanvasForEnvy");
                    if (canvasForEnvy != null)
                    {
                        canvasForEnvy.GetComponentInChildren<Canvas>().sortingOrder = 9999;
                    }
                }
                
                bool isNotBootstrapOrIntro = SceneHelper.CurrentScene != "Bootstrap" && SceneHelper.CurrentScene != "Intro";
                bool isMainMenu = SceneHelper.CurrentScene == "Main Menu";

                if (!Directory.Exists(EnvyUtility.ConfigPath))
                    Directory.CreateDirectory(EnvyUtility.ConfigPath);
                
                if (s.name != (LevelLoader.CurrentLevel?.Name ?? ""))
                {
                    LevelLoader.IsCustomLevel = false;
                    Debug.Log("Not envy level");
                }
                
                if (isMainMenu)
                {
                    ShaderManager.CreateShaderDictionary();
                    
                    ResourceLoader.PreloadGameobjectAtAddressAsync("FirstRoom Player Only");
                    ResourceLoader.PreloadGameobjectAtAddressAsync("FirstRoom");
                    ResourceLoader.PreloadGameobjectAtAddressAsync("FirstRoom Secret");
                    ResourceLoader.PreloadGameobjectAtAddressAsync("FirstRoom Prime");
                    ResourceLoader.PreloadGameobjectAtAddressAsync("FirstRoom Pit");
                    ResourceLoader.PreloadGameobjectAtAddressAsync("Assets/Prefabs/Levels/Special Rooms/FirstRoom Encore.prefab");
                    ResourceLoader.PreloadGameobjectAtAddressAsync("Assets/Prefabs/Levels/Special Rooms/FinalRoom Encore.prefab");
                    ResourceLoader.PreloadGameobjectAtAddressAsync("Assets/Prefabs/Levels/Special Rooms/FinalRoom.prefab");
                    ResourceLoader.PreloadGameobjectAtAddressAsync("Assets/Prefabs/Levels/Shop.prefab");
                }

                if (menuPrefab == null)
                { Debugger.LogWarn("menuPrefab is null"); return; }
                if (iconPrefab == null)
                { Debugger.LogWarn("iconPrefab is null"); return; }
                if (canvasForEnvy == null)
                { Debugger.LogWarn("canvasForEnvy is null"); return; }

                if (isMainMenu)
                {
                    GameObject target = EnvyUtility.FindObjectEvenIfDisabled("Canvas", "Chapter Select");
                    LoadEnvyMenu(target);
                }
                else
                {
                    // delayed to let player to load in
                    Action a = new Action(() => Debugger.LogError("Action a failed to setup??"));
                    a = () =>
                    {
                        GameObject target = EnvyUtility.FindObjectEvenIfDisabled("Canvas", "PauseMenu");
                        if (target == null)
                        {
                            PauseMenu[] pauseMenu = Resources.FindObjectsOfTypeAll<PauseMenu>();
                            Scene s = SceneManager.GetActiveScene();
                            foreach (var p in pauseMenu)
                            {
                                if(p.gameObject.scene != s)
                                    continue;
                                if(p.gameObject.scene.name != s.name)
                                    continue;
                                if((p.transform.parent?.name ?? "").ToLower().Contains("canvas"))
                                    target = p.gameObject;
                            }
                        }
                        if (target == null)
                        {
                            EnvyUtility.RunOnMainThread(a, 0.25f);
                        }

                        LoadEnvyMenu(target);
                    };
                    EnvyUtility.RunOnMainThread(a, 0.25f);
                }
            };
            Harmony.PatchAll();
            Debug.Log("Loaded envy level loader!");
        }

        private void LoadEnvyMenu(GameObject target)
        {
            if (target == null)
            {
                Debugger.LogWarn("target is null");
                return;
            }
            var canvasForEnvyInstance = Instantiate(canvasForEnvy, null);
            currentMenuInstance = GameObject.Instantiate(menuPrefab, canvasForEnvyInstance.transform, false);
            currentMenuInstance.SetActive(false);
            currentIconInstance = GameObject.Instantiate(iconPrefab, target.transform, false);
            currentIconInstance.SetActive(true);
            currentIconInstance.GetComponentInChildren<Button>().onClick = new Button.ButtonClickedEvent();
            currentIconInstance.GetComponentInChildren<Button>().onClick.AddListener(() =>
            {
                Debugger.Log("opening envy menu");
                currentMenuInstance.SetActive(true);
            });
        }

        public async Task GetTicket() //TODO: use this for custom leaderboards
        {
            NetIdentity id = new Steamworks.Data.NetIdentity();
            AuthTicket _ticketTask = await SteamUser.GetAuthSessionTicketAsync(id);

            byte[] zba = _ticketTask.Data;
            StringBuilder hex = new StringBuilder(zba.Length * 2);
            foreach (byte b in zba)
            {
                hex.AppendFormat("{0:x2}", b);
            }
        }

    }
}
