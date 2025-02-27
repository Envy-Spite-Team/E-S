using System;
using System.Collections;
using EnvyLevelLoader.Parsers;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq.Expressions;
using System.Threading;
using EnvyLevelLoader.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using Object = System.Object;

namespace EnvyLevelLoader.Loaders
{
    /// <summary>
    /// Class to load EnvyLevel classes.
    /// Holds data about the current level.
    /// </summary>
    public static class LevelLoader
    {
        public static bool IsCustomLevel     { get; private set; }
        public static bool IsOnlineLevel     { get; private set; }
        public static bool IsCampaginLevel   { get; private set; }
        public static EnvyLevel CurrentLevel { get; private set; }

        public const char SplitChar = '~';
        
        public static string GetLevelKey(EnvyLevel level, string sceneOverride = "")
        {
            if (string.IsNullOrEmpty(sceneOverride))
                sceneOverride = EnvyUtility.UnknownScene;

            return $"{EnvyUtility.EnvyScenePrefix}{Path.GetFileName(level.FilePath)}{SplitChar}{sceneOverride}";
        }

        public class Dummy : MonoBehaviour{}
        
        /// <summary>
        /// Loads a EnvyLevel. Used internally, see <see cref="GetLevelKey"/> and <see cref="SceneHelper.LoadScene"/> for loading levels manually.
        /// </summary>
        /// <param name="levelTarget">The level to load.</param>
        /// <returns>If the level is a valid level.</returns>
        public static bool LoadLevel(EnvyLevel levelTarget, string targetScene = "")
        {
            if(levelTarget == null)
                return false;
            
            if (string.IsNullOrWhiteSpace(targetScene))
                targetScene = EnvyUtility.UnknownScene;
            
            // don't reload bundle if we are restarting level
            bool canUnloadAndLoad = true;
            if (CurrentLevel != null)
            {
                if (CurrentLevel.FilePath == levelTarget.FilePath)
                {
                    canUnloadAndLoad = false;
                    if (levelTarget.LoadedBundle == null)
                    {
                        Debugger.Log("Fixing levelTarget's bundle to match the existing already loaded one...");
                        levelTarget.LoadedBundle = CurrentLevel.LoadedBundle;
                    }
                }
            }
            
            if(canUnloadAndLoad)
            {
                if (CurrentLevel != null)
                    CurrentLevel?.LoadedBundle?.Unload(true);
                if(levelTarget.LoadedBundle != null)
                    levelTarget.LoadedBundle?.Unload(true);
                levelTarget.LoadedBundle = AssetBundle.LoadFromMemory(levelTarget.BundleData);
            }
            else
            {
                Debugger.Log("Using already loaded level bundle...");
            }

            Debugger.Log($"Loading envy level {levelTarget.Name} with scene {targetScene}");

            Debugger.Log("Got bundle as " + levelTarget.LoadedBundle);
            if(targetScene == EnvyUtility.UnknownScene)
                targetScene = levelTarget.LoadedBundle.GetAllScenePaths().FirstOrDefault();

            IsOnlineLevel = false; // TODO : LINK TO ENVYDL
            IsCustomLevel = true;
            CurrentLevel = levelTarget;

            var loadingScreenOG = Plugin.menu.LoadAsset<GameObject>("LoadingALevelBlocker");
            LoadingLevelsBlocker info = null;
            if (loadingScreenOG != null && EnvySettingsManager.GetBool("enableLoadingScreen", true))
            {
                Debugger.Log("Loading loading screen...");
                var canvasForEnvyInstance = UnityEngine.Object.Instantiate(Plugin.canvasForEnvy, null);
                var loadingScreen = UnityEngine.Object.Instantiate(loadingScreenOG, canvasForEnvyInstance.transform);
                loadingScreen.SetActive(true);
                UnityEngine.Object.DontDestroyOnLoad(canvasForEnvyInstance);
                info = loadingScreen.GetComponentInChildren<LoadingLevelsBlocker>();
                Debugger.Log($"{loadingScreenOG} -> {loadingScreen} and {info}");
            }
            
            var asyncOp = SceneManager.LoadSceneAsync(targetScene);
            asyncOp!.allowSceneActivation = false;
            asyncOp!.completed += op =>
            {
                IsCustomLevel = true;
                SceneHelper.DismissBlockers();
                
                // fix ultrakill stuff
                try
                {
                    StockMapInfo info = UnityEngine.Object.FindObjectOfType<StockMapInfo>();
                    
                    bool canOLS = true;
                    foreach (OnLevelStart ols in Resources.FindObjectsOfTypeAll<OnLevelStart>())
                    {
                        if (ols.gameObject.scene == SceneManager.GetActiveScene())
                        {
                            canOLS = false;
                        }
                    }

                    if (canOLS)
                    {
                        OnLevelStart onLevelStart = info.gameObject.AddComponent<OnLevelStart>();
                        onLevelStart.onStart = new UltrakillEvent();
                        onLevelStart.hideFogUntilStart = false;
                        onLevelStart.fogHidden = false;
                    }
                }catch(Exception){}
                
                // start appling shaders
                var dummy = new GameObject("tmp").AddComponent<Dummy>();
                dummy.StartCoroutine(ShaderManager.ApplyShadersAsyncContinuously());
                
                Camera mainCamera = Camera.main;
                if(mainCamera != null)
                    mainCamera.clearFlags = CameraClearFlags.Skybox;

                if (Path.GetFileName(levelTarget.FilePath) == Path.GetFileName(EnvyUtility.CreditsLevelPath))
                {
                    EnvyUtility.RunOnMainThread(() =>
                    {
                        Debugger.Log("Fixing credits level font....");
                        // we are in credits level so lets fix the font rq
                        Material fixedMaterial = Plugin.menu.LoadAsset<Material>("CreditsFontMaterial");
                        fixedMaterial = UnityEngine.Object.Instantiate(fixedMaterial);
                        TMP_FontAsset fixedFont = Plugin.menu.LoadAsset<TMP_FontAsset>("CreditsFontTMP");
                        fixedFont = UnityEngine.Object.Instantiate(fixedFont);
                        
                        TextMeshProUGUI[] allText = Resources.FindObjectsOfTypeAll<TextMeshProUGUI>();
                        Scene current = SceneManager.GetActiveScene();
                        foreach (var text in allText)
                        {
                            if(text.gameObject.scene != current)
                                continue;
                            if(text.gameObject.scene.name != current.name)
                                continue;
                        
                            text.material = fixedMaterial;
                            text.font = fixedFont;
                        }
                    }, 0.125f);
                }
            };
            if (info != null)
            {
                info.StartCoroutine(INTERNAL_LoadingScreen(asyncOp, info));
            }
            else
            {
                asyncOp!.allowSceneActivation = true;
            }
            
            return true;
        }

        private const int loadingSize = 24;
        static IEnumerator INTERNAL_LoadingScreen(AsyncOperation asyncOp, LoadingLevelsBlocker info)
        {
            while (asyncOp.progress < 0.899f)
            {
                if (MonoSingleton<OptionsManager>.Instance != null)
                {
                    if (!MonoSingleton<OptionsManager>.Instance.paused)
                    {
                        MonoSingleton<OptionsManager>.Instance.Pause();
                        MonoSingleton<OptionsManager>.Instance.dontUnpause = true;
                    }
                }
                yield return null;
                info.Text.text = "[";
                for (int i = 0; i < loadingSize * (asyncOp.progress+0.09f); i++)
                {
                    info.Text.text += "*";
                }

                for (int i = 0; i < loadingSize * (1 - (asyncOp.progress+0.09f)); i++)
                {
                    info.Text.text += "-";
                }
                info.Text.text += "]";
            }
            asyncOp.allowSceneActivation = true;
            UnityEngine.Object.Destroy(info.GetComponentInParent<Transform>().gameObject, 0.125f);
        }

        /// <summary>
        /// Gets an EnvyLevel from a file, can be .doomah or .envy.
        /// </summary>
        /// <param name="path">The file to get the EnvyLevel from.</param>
        /// <returns>The EnvyLevel. (can be null)</returns>
        public static EnvyLevel GetLevelFromFile(string path)
        {
            bool isDoomah = Path.GetExtension(path).ToLower() == ".doomah";

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Read);

                EnvyLevel level = null;

                if (isDoomah)
                    level = DoomahParser.ParseLevelInfo(archive);
                else
                    level = EnvyParser.ParseLevelInfo(archive);

                if(level == null) return null;

                level.FilePath = path;

                return level;
            }
        }
    }
}
