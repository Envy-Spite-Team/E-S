using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;
using Unity.Collections;
using System;
using Mono.Security.Cryptography;
using System.IO;
using System.Net;
using System.Text;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using UnityEngine.UI.Extensions;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;

namespace DoomahLevelLoader
{
    public class Parameter
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Img { get; set; }
        public List<string> Urls { get; set; }
        public List<byte[]> UrlsHash { get; set; }
        public string Description { get; set; }
        public string Version { get; set; }
        public bool Serious { get; set; }
    }

    public class EnvyDownloaderMenu : MonoBehaviour
    {
        private List<Parameter> parameters;

        public GameObject ContentFromScrollview;
        public GameObject FiltersBox;
        public GameObject OnlineLevelButton;
        public GameObject LevelPopUp;
        public TMP_InputField SearchBar;

        [HideInInspector]
        public int Count;

        private string JsonURL = "https://raw.githubusercontent.com/SatisfiedBucket/EnvySpiteDownloader/main/Networking/EnvyDL.json";

        private string urlLoadResult;
        private byte[] urlImgLoadResult;

        public void Refresh()
        {
            StartCoroutine(LoadURL());
        }

        public async void InstallButton(List<string> DownloadURLS, List<byte[]> DownloadHASH, TextMeshProUGUI InstallText, string Name)
        {
            Count = 0;
            foreach (string DownloadURL in DownloadURLS)
            {
                Count++;
                string fileName = System.IO.Path.GetTempPath() + name + ".doomah.part" + Count;
                await StartDownload(DownloadURL, fileName, InstallText, Count, DownloadURLS.Count);
                if (File.Exists(fileName))
                {
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.OpenRead(fileName))
                        {
                            byte[] Hash = md5.ComputeHash(stream);
                            if (Hash != DownloadHASH[Count - 1])
                            {
                                Debug.LogError("Invalid hash, cancelling download.");
                            }
                            else
                            {
                                File.Move(fileName, Plugin.getConfigPath() + name + ".doomah.part" + Count);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogError("Downloaded file does not exist!");
                    return;
                }
            }
            EnvyLoaderMenu.Instance.RefreshOnOpen = true;
        }

        static async Task StartDownload(string url, string name, TextMeshProUGUI InstallText, int count, int max)
        {
            WebClient webcl = new WebClient();
            webcl.DownloadFileCompleted += DownloadCompleted;
            webcl.DownloadProgressChanged += (sender, e) => DownloadProgressChanged(sender, e, InstallText, count, max);
            await Task.Run(() =>
            {
                webcl.DownloadFileAsync(new Uri(url), name);
            });
        }

        public static void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e, TextMeshProUGUI InstallText, int count, int max)
        {
            InstallText.text = e.ProgressPercentage.ToString() + " - " + count + "/" + max;
        }
        public static void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {

        }

            public void AboutButton(string Name, string Description)
        {
            if (LevelPopUp)
            {
                LevelPopUp popUpInstance = LevelPopUp.GetComponent<LevelPopUp>();
                GameObject gameObject = popUpInstance.gameObject;
                popUpInstance.LevelName.text = Name;
                popUpInstance.LevelDescription.text = Description;
                gameObject.SetActive(true);
            }
        }

        private IEnumerator LoadURL()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get(JsonURL))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debug.LogError("Failed to fetch Level List: " + webRequest.error);
                    urlLoadResult = null;
                }
                else
                {
                    urlLoadResult = webRequest.downloadHandler.text;
                }
            }

            parameters = JsonConvert.DeserializeObject<List<Parameter>>(urlLoadResult);

            foreach (Transform child in ContentFromScrollview.transform)
            {
                Destroy(child.gameObject);
            }

            foreach (var parameter in parameters)
            {
                GameObject newButton = Instantiate(OnlineLevelButton);
                OnlineLevelButton component = newButton.GetComponent<OnlineLevelButton>();

                newButton.transform.parent = ContentFromScrollview.transform;

                component.LevelName.text = parameter.Name;
                component.LevelAuthor.text = parameter.Author;
                component.LevelDescription = parameter.Description;

                component.InstallButton.onClick.AddListener(() => InstallButton(parameter.Urls, parameter.UrlsHash, component.InstallButton.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>(), parameter.Name));
                component.AboutButton.onClick.AddListener(() => AboutButton(parameter.Name, parameter.Description));

                Texture2D levelImage = new Texture2D(2, 2);
                using (UnityWebRequest webRequest = UnityWebRequest.Get(parameter.Img))
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.isNetworkError || webRequest.isHttpError)
                    {
                        Debug.LogError("Failed to fetch Level List: " + webRequest.error);
                        urlLoadResult = null;
                    }
                    else
                    {
                        urlImgLoadResult = webRequest.downloadHandler.data;
                    }
                }
                if (urlImgLoadResult != null)
                {
                    byte[] imageBytes = urlImgLoadResult;
                    if (levelImage.LoadImage(imageBytes))
                    {
                        component.ActualImage.sprite = Sprite.Create(levelImage, new Rect(0, 0, levelImage.width, levelImage.height), Vector2.zero);
                        component.ActualImage.color = Color.white;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError($"Failed to load image for:" + parameter.Name);
                    }
                }
                else
                {
                    Debug.Log("Failed to load image, skipping");
                }
                Debug.Log("Loaded " + parameter.Name + "!");
            }
        }
    }
}