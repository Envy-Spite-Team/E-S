using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using Newtonsoft.Json;
using System.Reflection;
using UnityEngine.Networking;
using System.Threading;

namespace DoomahLevelLoader
{
    public class EnvyLoaderMenu : MonoBehaviour
    {
        private static EnvyLoaderMenu instance;

        public GameObject ContentStuff;
        public GameObject LevelsMenu;
        public GameObject LevelsButton;
        public GameObject FuckingPleaseWait;
        public TextMeshProUGUI MOTDMessage;
        public Image thatfuckingemojithatihate;

        [HideInInspector]
        public bool RefreshOnOpen;

        public static EnvyLoaderMenu Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<EnvyLoaderMenu>();
                    if (instance == null)
                    {
                        Debugger.LogError("EnvyScreen prefab not found or EnvyLoaderMenu instance not initialized.");
                        return null;
                    }
                }
                return instance;
            }
        }

        public void Start()
        {
            EnvyLoaderMenu.CreateLevels();
            StartCoroutine(MOTDManager.LoadMOTD(MOTDMessage, thatfuckingemojithatihate));
        }

        public void Update()
        {
            if (RefreshOnOpen)
            {
                if (instance.gameObject.activeSelf)
                {
                    Loaderscene.RefreshLevels();
                    RefreshOnOpen = false;
                }
            }
        }

        public static void CreateLevels()
        {
            foreach (AssetBundleInfo bundleInfo in Loaderscene.AssetBundles)
            {
                GameObject levelButtonObject = Instantiate(Instance.LevelsButton, Instance.ContentStuff.transform);
                LevelButtonScript buttonScript = levelButtonObject.GetComponent<LevelButtonScript>();

                Loaderscene.SetLevelButtonScriptProperties(buttonScript, bundleInfo);

                if (bundleInfo.IsCampaign)
                {
                    buttonScript.SceneToLoad = "";
                }

                buttonScript.LevelButtonReal.onClick.AddListener(() => Loaderscene.LoadScene(buttonScript));
            }
        }

        public static void ClearContentStuffChildren()
        {
            if (Instance == null) return;

            foreach (Transform child in Instance.ContentStuff.transform)
            {
                Destroy(child.gameObject);
            }
        }

        public static IEnumerator UpdateLevelListingCoroutine()
        {
            Instance.FuckingPleaseWait.SetActive(true);

            ClearContentStuffChildren();
            CreateLevels();

            yield return null;

            Instance.FuckingPleaseWait.SetActive(false);
        }

        public static void UpdateLevelListing()
        {
            if (Instance != null)
            {
                Instance.StartCoroutine(UpdateLevelListingCoroutine());
            }
        }
    }

    public class DropdownHandler : MonoBehaviour
    {
        public TMP_Dropdown dropdown;

        private const string selectedDifficultyKey = "difficulty";
        private string settingsFilePath;

        private void Awake()
        {
            settingsFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");
        }

        private void OnEnable()
        {
            int savedDifficulty = LoadDifficulty();
            dropdown.value = savedDifficulty;
            dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
        }

        private int LoadDifficulty()
        {
            return MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
        }

        public void OnDropdownValueChanged(int index)
        {
            Debugger.Log("Difficulty set to " + index);
            MonoSingleton<PrefsManager>.Instance.SetInt("difficulty", index);
        }
    }
}