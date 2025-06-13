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
using DoomahLevelLoader;
using EnvyLevelLoader.UI;
using TMPro;
using UnityEngine.AddressableAssets;
using UnityEngine.AI;
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
        public static bool IsCustomLevel     { get; internal set; }
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
        /// <param name="targetScene">The scene within the level to load.</param>
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
                if (CurrentLevel.FilePath == levelTarget.FilePath && CurrentLevel.EditedDate == levelTarget.EditedDate)
                {
                    canUnloadAndLoad = false;
                    if (levelTarget.LoadedBundle == null)
                    {
                        Debugger.Log("Fixing levelTarget's bundle to match the existing already loaded one...");
                        levelTarget.LoadedBundle = CurrentLevel.LoadedBundle;
                    }
                }
            }
            
            if (MonoSingleton<OptionsManager>.Instance != null)
            {
                if (!MonoSingleton<OptionsManager>.Instance.paused)
                {
                    MonoSingleton<OptionsManager>.Instance.Pause();
                    MonoSingleton<OptionsManager>.Instance.dontUnpause = true;
                }
            }

            if(!EnvySettingsManager.GetBool("enableLoadingScreen", true))
                SceneHelper.ShowLoadingBlocker();

            if (!canUnloadAndLoad)
            {
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
                    info.Text.text = "[ LOADING LEVEL DATA ]";
                    Debugger.Log($"{loadingScreenOG} -> {loadingScreen} and {info}");
                }
            
                EnvyUtility.RunOnMainThread(() =>
                {
                    if(info != null)
                        UnityEngine.Object.DestroyImmediate(info.GetComponentInParent<Transform>().gameObject);
                    INTERNAL_LoadLevel(levelTarget, targetScene, canUnloadAndLoad);
                }, 0.125f);
            }
            else
            {
                INTERNAL_LoadLevel(levelTarget, targetScene, canUnloadAndLoad);
            }
            
            return true;
        }

        static void INTERNAL_LoadLevel(EnvyLevel levelTarget, string targetScene = "", bool canUnloadAndLoad = false)
        {
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

            bool ignoreOldDoomah = levelTarget.LoadedBundle.isStreamedSceneAssetBundle;
            if (levelTarget is PrefabDoomahParser.PrefabEnvyLevel && !ignoreOldDoomah)
            {
                Debugger.Log("Current level is a prefab level, applying fixes...");
                targetScene = "this is not a scene";
            }
            if(targetScene == EnvyUtility.UnknownScene)
                targetScene = levelTarget.LoadedBundle.GetAllScenePaths().FirstOrDefault();

            IsOnlineLevel = false; // TODO: LINK TO ENVYDL
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
            
            if (levelTarget is PrefabDoomahParser.PrefabEnvyLevel && !ignoreOldDoomah)
            {
                if (info)
                    info.Text.text = "[ LOADING SANDBOX ]";
                Addressables.LoadSceneAsync("uk_construct")!.Completed += (op) =>
                {
                    if (info)
                        UnityEngine.Object.Destroy(info.GetComponentInParent<Transform>().gameObject, 0.125f);
                    IsCustomLevel = true;
                    GameObject sandboxMap = CurrentLevel.LoadedBundle.LoadAsset<GameObject>("MapBase");
                    GameObject sandboxMapClone = UnityEngine.Object.Instantiate(sandboxMap,  new Vector3(0f, 300f, 0f), Quaternion.identity);
                    NavMeshSurface nms = sandboxMapClone.AddComponent<NavMeshSurface>();
                    
                    // start appling shaders
                    var dummy = new GameObject("tmp").AddComponent<Dummy>();
                    dummy.StartCoroutine(ShaderManager.ApplyShadersAsyncContinuously());
                    
                    EnvyUtility.RunOnMainThread(() =>
                    {
                        var challengeText = EnvyUtility.FindObjectEvenIfDisabled("Player",
                            "Main Camera/HUD Camera/HUD/FinishCanvas/Panel/Challenge/ChallengeText");
                        if (challengeText != null && !ChallengeInfo.HasRanThisScene)
                        {
                            challengeText.GetComponentInChildren<TextMeshProUGUI>()!.text = "NO CHALLENGE AVAILABLE FOR THIS LEVEL";
                        }
                        
                        Scene s = SceneManager.GetActiveScene();
                        Collider[] colliders = Resources.FindObjectsOfTypeAll<Collider>();
                        for (int i = 0; i < colliders.Length; i++)
                        {
                            Collider c = colliders[i];
                            GameObject g = c.gameObject;
                            if(g == null) continue;
                            if(g.scene != s) continue;
                            if (g.transform.root != sandboxMapClone.transform) continue;
                            
                            // fix broken objects
                            if (g.CompareTag("Body") && (g.GetComponentInParent<EnemyIdentifierIdentifier>() == null || g.GetComponentInParent<EnemyIdentifier>() == null))
                            {
                                g.tag = "Floor";
                                g.layer = LayerMask.NameToLayer("Environment");
                            }
                        }
                        
                        nms.BuildNavMesh();
                    }, 0.1f);
                };
            }
            else
            {
                AsyncOperation asyncOp = SceneManager.LoadSceneAsync(targetScene);
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
                    
                    EnvyUtility.RunOnMainThread(() =>
                    {
                        var challengeText = EnvyUtility.FindObjectEvenIfDisabled("Player",
                            "Main Camera/HUD Camera/HUD/FinishCanvas/Panel/Challenge/ChallengeText");
                        if (challengeText != null && !ChallengeInfo.HasRanThisScene)
                        {
                            challengeText.GetComponentInChildren<TextMeshProUGUI>()!.text = "NO CHALLENGE AVAILABLE FOR THIS LEVEL";
                        }
                    }, 0.125f);
                    
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
            }
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
            if (!EnvyUtility.IsZipValid(path))
            {
                Debug.LogWarning("Level at " + path + " seems to be a really old doomah file. Attempting to load via faking new methods.");
                var level = PrefabDoomahParser.ParseLevelInfo(path);
                level!.FilePath = path;
                return level;
            }
            
            bool isDoomah = Path.GetExtension(path).ToLower() == ".doomah";
            
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Read);

                EnvyLevel level = null;

                if (isDoomah)
                    level = DoomahParser.ParseLevelInfo(archive, Path.GetFileName(path));
                else
                    level = EnvyParser.ParseLevelInfo(archive);

                if(level == null) return null;

                level.FilePath = path;

                return level;
            }
        }
    }
}
