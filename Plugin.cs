using BepInEx;
using EnvyLevelLoader.Loaders;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

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
        public static GameObject currentMenuInstance;

        public static Plugin Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            menu = ResourceLoader.GetBundle("envymenu");
            menuPrefab = menu.LoadAsset<GameObject>("EnvyLoader");
            SceneManager.sceneLoaded += (Scene s, LoadSceneMode lcm) =>
            {
                bool isNotBootstrapOrIntro = SceneHelper.CurrentScene != "Bootstrap" && SceneHelper.CurrentScene != "Intro";
                bool isMainMenu = SceneHelper.CurrentScene == "Main Menu";

                if (isNotBootstrapOrIntro)
                    ShaderManager.CreateShaderDictionary();

                if(isMainMenu && false)
                    LevelLoader.LoadLevel(LevelLoader.GetLevelFromFile("C:\\Users\\Leo\\AppData\\Roaming\\r2modmanPlus-local\\ULTRAKILL\\profiles\\envydev\\BepInEx\\config\\EnvyLevels\\ub_templeoftime.doomah"), EnvyUtility.UnknownScene);

                if (ShaderManager.shaderDictionary.Count <= 0)
                    StartCoroutine(ShaderManager.LoadShadersAsync());

                if (menuPrefab == null)
                { Debugger.LogWarn("menuPrefab is null"); return; }

                GameObject target = EnvyUtility.FindObjectEvenIfDisabled("Canvas", "Main Menu (1)");

                if(target == null)
                    target = EnvyUtility.FindObjectEvenIfDisabled("Canvas", "PauseMenu");

                if (target == null)
                    return;

                currentMenuInstance = GameObject.Instantiate(menuPrefab);
                currentMenuInstance.transform.SetParent(target.transform, false);
            };
            Harmony.PatchAll();
        }
    }
}
