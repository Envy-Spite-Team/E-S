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
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
using static Mono.Security.X509.X520;

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

        /// <summary>
        /// Creates a LevelUI from an envy level then adds it to the level list.
        /// </summary>
        /// <param name="level">The level to create a LevelUI from.</param>
        public void AddLevel(EnvyLevel level)
        {
            GameObject newUIObject = GameObject.Instantiate(BaseUI.gameObject);
            LevelUI levelUI = newUIObject.GetComponent<LevelUI>();
            levelUI.Load(level);

            newUIObject.transform.SetParent(Container.transform, false);

            levels.Add(levelUI);
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

            if (!Directory.Exists(path))
                Debugger.LogError($"Invalid path to load levels from (got {path})");
            
            StartCoroutine(LoadLevelsInternal(path, blocker));
        }

        IEnumerator LoadLevelsInternal(string path, bool blocker)
        {
            string[] filePaths = Directory.GetFiles(path).Where((name) =>
            {
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
                    blockerGO = GameObject.Instantiate(blockerGO) as GameObject;
                    Title = blockerGO.transform.Find("Title").GetComponent<TextMeshProUGUI>();
                    Info = blockerGO.transform.Find("Info").GetComponent<TextMeshProUGUI>();

                    blockerGO.transform.SetParent(myCanvas.transform, false);
                    blockerGO.SetActive(true);
                }
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
                EnvyLevel level = LevelLoader.GetLevelFromFile(file);
                string error = $"<color=green>{Path.GetFileName(file)}</color>";

                if (level != null)
                    AddLevel(level);
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

            //TODO: add a script for this
            MOTDManager.LoadMOTD(GameObject.Find("Canvas/Main Menu (1)/EnvyLoader(Clone)/LevelsTab/MOTD/VeryEpicCoolMOTDMsgThisNameIsVerySpecificSoICanFindIt").GetComponent<TextMeshProUGUI>(), new GameObject().AddComponent<Image>());
        }

        void Start()
        {
            if (AutoLoad)
                LoadLevelsFrom(EnvyUtility.ConfigPath);
        }
    }
}
