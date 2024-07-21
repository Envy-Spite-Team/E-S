using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using Newtonsoft.Json;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;
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
        private bool AlreadyStarted = false;

        public static EnvyLoaderMenu Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<EnvyLoaderMenu>();
                    if (instance == null)
                    {
                        Debug.LogError("EnvyScreen prefab not found in the terminal bundle. This error may look scary but it's probably nothing to worry about.");
                    }
                }
                if (instance.AlreadyStarted == false)
                {
                    instance.AlreadyStarted = true;
                    instance.Start();
                }
                return instance;
            }
        }

        public void Start()
        {
            EnvyLoaderMenu.CreateLevels();

            // Debug.Log("WAS THAT THE ERROR OF 87?");
            // UnityWebRequest www = UnityWebRequest.Get("https://raw.githubusercontent.com/SatisfiedBucket/EnvySpiteDownloader/main/motd.txt");
            // Debug.Log("Test 0");
            // yield return www.SendWebRequest();

            // Debug.Log("Test 1");
            // if (www.isNetworkError || www.isHttpError)
            // {
                // Debug.Log(www.error);
                // Debug.Log("Test 2");
            // }
            // else
            // {
                // Show results as text
                // Debug.Log(www.downloadHandler.text);

              //  Or retrieve results as binary data
                // byte[] results = www.downloadHandler.data;

                // Debug.Log("Shit is about to get real.");
                // if (www.downloadHandler.text != null)
                // {
                    // Debug.Log(www.downloadHandler.text);
                    // Instance.MOTDMessage.text = www.downloadHandler.text;
                // }
                // else
                // {
                    // Debug.LogError("HTTP request failed, check if you even have WIFI.");
                // }
            // }
        }

        public static void CreateLevels()
        {

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
            if (File.Exists(settingsFilePath))
            {
                string json = File.ReadAllText(settingsFilePath);
                Dictionary<string, int> settings = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);

                if (settings != null && settings.TryGetValue(selectedDifficultyKey, out int difficulty))
                {
                    return difficulty;
                }
            }
            return 2;
        }

        private void SaveDifficulty(int difficulty)
        {
            Dictionary<string, int> settings = new Dictionary<string, int>
            {
                { selectedDifficultyKey, difficulty }
            };

            string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(settingsFilePath, json);
        }

        public void OnDropdownValueChanged(int index)
        {
            SaveDifficulty(index);
        }
    }
}
