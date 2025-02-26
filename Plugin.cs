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
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace EnvyLevelLoader
{
    [BepInPlugin(modGUID, modName, modVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string modGUID = "envyandspite.ultrakill.envylevelloader";
        private const string modName = "envylevelloader";
        private const string modVersion = "2.0.0";

        private static readonly Harmony Harmony = new Harmony(modGUID);

        public static AssetBundle menu;
        public static GameObject menuPrefab;
        public static GameObject iconPrefab;
        public static GameObject canvasForEnvy;
        public static GameObject currentMenuInstance;
        public static GameObject currentIconInstance;

        public static Plugin Instance { get; private set; }

        public static GameObject FirstRoomTemp;
        public static GameObject PlayerTemp;
        public static GameObject ShopTemp;
        public static GameObject FinalRoomTemp;
        
        private void Awake()
        {
            EnvyUtility.CaptureMainThread();
            Instance = this;

            menu = ResourceLoader.GetBundle("envymenu");
            
            Debugger.Log("Testing bundle integrity...");
            Object[] bundleObjects = Plugin.menu.LoadAllAssets();
            foreach (Object obj in bundleObjects)
            {
                Debug.Log($"Found {obj.name} [{obj.GetType().FullName}] in bundle {menu.name}.");
            }
            Debugger.Log("____________________");
            
            menuPrefab = menu.LoadAsset<GameObject>("EnvyMenu");
            iconPrefab = menu.LoadAsset<GameObject>("EnvyIcon");
            canvasForEnvy = menu.LoadAsset<GameObject>("CanvasForEnvy");
            if (canvasForEnvy != null)
            {
                canvasForEnvy.GetComponentInChildren<Canvas>().sortingOrder = 9999;
            }
            SceneManager.sceneLoaded += (Scene s, LoadSceneMode lcm) =>
            {
                bool isNotBootstrapOrIntro = SceneHelper.CurrentScene != "Bootstrap" && SceneHelper.CurrentScene != "Intro";
                bool isMainMenu = SceneHelper.CurrentScene == "Main Menu";

                if (isMainMenu)
                {
                    ShaderManager.CreateShaderDictionary();
                }

                //testing code
                if(isMainMenu)
                {
                    Debugger.Log("Loading envy user! " + SceneHelper.CurrentScene);
                    EnvyUser user = new EnvyUser(76561197960435530);
                    Task.Run(async () =>
                    {
                        Debugger.Log("Waiting for user.HasLoaded");
                        while (!user.HasLoaded)
                            await Task.Yield();
                        Debugger.Log("EnvyUser loaded! " + user.HasLoaded.ToString());
                    });

                    Addressables.LoadAssetAsync<GameObject>("FirstRoom").Completed += handle =>
                    {
                        FirstRoomTemp = handle.Result;
                        Debugger.Log("Got FirstRoom as " + handle.Result);
                    };
                    Addressables.LoadAssetAsync<GameObject>("FirstRoom Player Only").Completed += handle =>
                    {
                        PlayerTemp = handle.Result;
                        Debugger.Log("Got FirstRoom Player Only as " + handle.Result);
                    };
                    Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Levels/Shop.prefab").Completed += handle =>
                    {
                        ShopTemp = handle.Result;
                        Debugger.Log("Got Assets/Prefabs/Levels/Shop.prefab Only as " + handle.Result);
                    };
                    Addressables.LoadAssetAsync<GameObject>("Assets/Prefabs/Levels/Special Rooms/FinalRoom.prefab").Completed += handle =>
                    {
                        FinalRoomTemp = handle.Result;
                        Debugger.Log("Got Assets/Prefabs/Levels/Special Rooms/FinalRoom.prefab Only as " + handle.Result);
                    };
                }

                if (menuPrefab == null)
                { Debugger.LogWarn("menuPrefab is null"); return; }
                if (iconPrefab == null)
                { Debugger.LogWarn("iconPrefab is null"); return; }
                if (canvasForEnvy == null)
                { Debugger.LogWarn("canvasForEnvy is null"); return; }

                GameObject target = EnvyUtility.FindObjectEvenIfDisabled("Canvas", "Chapter Select");

                if (target == null)
                    target = EnvyUtility.FindObjectEvenIfDisabled("Canvas", "PauseMenu");

                if (target == null)
                    return;

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
            };
            Harmony.PatchAll();
        }

        public async Task GetTicket() //TODO : use this for custom leaderboards
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
