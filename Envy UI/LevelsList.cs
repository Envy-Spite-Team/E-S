using EnvyLevelLoader.Loaders;
using EnvyLevelLoader.Parsers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnvyLevelLoader.UnityComponents;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace EnvyLevelLoader.UI
{
    public class LevelsList : MonoBehaviour
    {
        public GameObject Container;
        public LevelUI BaseUI;
        public GameObject NoLevels;
        [HideInInspector]
        public List<LevelUI> levels = new List<LevelUI>();

        public bool AutoLoad = false;

        private static List<EnvyLevel> loadedLevels = new List<EnvyLevel>();
        public static List<EnvyLevel> LoadedLevels => loadedLevels;

        private GameObject fakeEnvyLights;

        public void Awake()
        {
            var coolLights = GameObject.Find("Pit (2)")?.transform?.Find("Agony Lights")?.gameObject ?? null;
            if(coolLights != null)
                fakeEnvyLights = Instantiate(coolLights);
        }

        public void Start()
        {
            if (AutoLoad)
                LoadLevelsFrom(EnvyUtility.ConfigPath);
        }

        public void Update()
        {
            if(Keyboard.current?.escapeKey.wasPressedThisFrame ?? false)
                this.gameObject.SetActive(false);
        }

        public void OnEnable()
        {
            if(fakeEnvyLights != null)
                fakeEnvyLights.SetActive(true);
        }

        public void OnDisable()
        {
            if(fakeEnvyLights != null)
                fakeEnvyLights.SetActive(false);
        }

        /// <summary>
        /// Creates a LevelUI from an envy level then adds it to the level list.
        /// </summary>
        /// <param name="level">The level to create a LevelUI from.</param>
        public GameObject AddLevel(EnvyLevel level, bool alreadyLoaded = false)
        {
            if (BaseUI == null)
            {
                var uiObj = Plugin.menu.LoadAsset<GameObject>("LevelUI");
                BaseUI = uiObj.GetComponent<LevelUI>();
                if (BaseUI == null)
                {
                    BaseUI = uiObj.AddComponent<LevelUI>();
                    BaseUI.Name = uiObj.transform.Find("Name").GetComponent<TextMeshProUGUI>();
                    BaseUI.Author = uiObj.transform.Find("Author").GetComponent<TextMeshProUGUI>();
                    BaseUI.RankIcon = uiObj.transform.Find("Rank Icon").GetComponentInChildren<RankIcon>();
                    BaseUI.Thumbnail = uiObj.transform.Find("Image").GetComponent<RawImage>();
                    BaseUI.LevelInfo = uiObj.transform.Find("Info").GetComponent<Button>();
                    BaseUI.ScriptsIcon = uiObj.transform.Find("Scripts").GetComponent<Image>();
                    
                    Debugger.LogWarn("fucking level ui didn't load");
                }
            }
            GameObject newUIObject = GameObject.Instantiate(BaseUI.gameObject, Container.transform, false);
            LevelUI levelUI = newUIObject.GetComponent<LevelUI>();
            levelUI.Load(level);
            if(!alreadyLoaded)
                loadedLevels.Add(level);

            levels.Add(levelUI);
            return newUIObject;
        }

        /// <summary>
        /// Loads all the .doomah and .envy files from a directory.
        /// </summary>
        /// <param name="path">The directory to search for level files.</param>
        public void LoadLevelsFrom(string path, bool blocker = true)
        {
            if (levels.Count > 0)
            {
                Debugger.LogError($"Clearing old levels.");
                foreach (LevelUI levelUI in levels)
                    Destroy(levelUI.gameObject);
                levels = new List<LevelUI>();
            }
            
            if (!Directory.Exists(EnvyUtility.ConfigPath))
                Directory.CreateDirectory(EnvyUtility.ConfigPath);
            
            if (!Directory.Exists(path))
                Debugger.LogError($"Invalid path to load levels from (got {path})");
            
            StartCoroutine(LoadLevelsInternal(path, blocker));
        }

        
        IEnumerator LoadLevelsInternal(string path, bool blocker)
        {
            List<GameObject> objectsToActivate = new List<GameObject>();
            string[] filePaths = Directory.GetFiles(path).Where((name) =>
            {
                // check if level is already loaded before trying to load it again
                foreach (var level in loadedLevels)
                {
                    if (Path.GetFileNameWithoutExtension(level.FilePath) == Path.GetFileNameWithoutExtension(name))
                    {
                        var newDate = File.GetLastWriteTime(level.FilePath);
                        if (newDate == level.EditedDate)
                        {
                            // add the already loaded levels back into the ui
                            objectsToActivate.Add(AddLevel(level, true));
                            Debugger.Log($"Skipping loading level {level.FilePath} because it wasn't changed since last load.");
                            return false;
                        }
                    }
                }
                if(Path.GetFileName(name) == Path.GetFileName(EnvyUtility.CreditsLevelPath))
                    return false;
                
                string ext = Path.GetExtension(name).ToLower();
                return ext == ".doomah" || ext == ".envy";
            }).ToArray();


            GameObject blockerGO = null;
            TextMeshProUGUI Title = null;
            TextMeshProUGUI Info = null;

            if (blocker)
            {
                Canvas myCanvas = GetComponentInParent<Canvas>();
                if (myCanvas != null)
                {
                    blockerGO = Plugin.menu.LoadAsset("LoadingLevelsBlocker") as GameObject;
                    if (blockerGO == null)
                        throw new Exception("WHAT THE FUCK (invalid envymenu.bundle)");
                    blockerGO = GameObject.Instantiate(blockerGO, myCanvas.transform, false) as GameObject;
                    LoadingLevelsBlocker blockerScript = blockerGO.GetComponent<LoadingLevelsBlocker>();
                    Title = blockerScript.Title;
                    Info = blockerScript.Text;

                    blockerGO.SetActive(true);
                }
            }

            if (blocker)
            {
                Title.text = $"<b>Combining level parts...</b>";
                Info.text = "";
                Info.autoSizeTextContainer = true;
            }
            bool canGo = false;
            int maxWait = 960*2;
            Task.Run(async () =>
            {
                await Merger.MergeFiles();
                canGo = true;
            });
            while ((!canGo) && maxWait > 0)
            {
                maxWait--;
                yield return new WaitForSecondsRealtime(0.125f/2.0f);
            }
            
            int l = 1;
            if (blocker)
            {
                Title.text = $"<b>Loading Levels 1/{filePaths.Length}</b>";
                Info.text = "";
                Info.autoSizeTextContainer = true;
            }
            
            foreach (string file in filePaths)
            {
                yield return new WaitForEndOfFrame();
                EnvyLevel level = null;
                try
                {
                    level = LevelLoader.GetLevelFromFile(file);
                }catch(Exception e){Debugger.LogWarn(e.Message);}
                
                string error = $"<color=green>{Path.GetFileName(file)}</color>";

                if (level != null)
                {
                    level.EditedDate = File.GetLastWriteTime(file);
                    objectsToActivate.Add(AddLevel(level));
                }
                else
                {
                    error = $"<color=red>{Path.GetFileName(file)} failed to load due to invalid info.</color>";
                    Debugger.LogError(error);
                }
                if (blocker)
                {
                    Title.text = $"<b>Loading Levels {l}/{filePaths.Length}</b>";
                    Info.text = error + "<br>" + Info.text;
                }
                l++;
                yield return new WaitForEndOfFrame();
            }
            
            if (blocker)
                Destroy(blockerGO);
            
            if(loadedLevels.Count != 0)
            {
                NoLevels.SetActive(false);
                if (Container.TryGetComponent<ObjectActivateInSequenceNoTimeScale>(
                        out ObjectActivateInSequenceNoTimeScale activateInSequence))
                {
                    activateInSequence.objectsToActivate = objectsToActivate.ToArray();
                    activateInSequence.coroutine = activateInSequence.StartCoroutine(activateInSequence.activationCoroutine());
                }
            }
            else
                NoLevels.SetActive(true);
        }
        
        public void Refresh()
        {
            LoadLevelsFrom(EnvyUtility.ConfigPath);
        }
    }
}
