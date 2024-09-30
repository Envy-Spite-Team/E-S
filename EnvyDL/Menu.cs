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
using System.Net.Http;
using System.Threading;
using System.Data.Common;

namespace DoomahLevelLoader
{
    public class Parameter
    {
        public string Name { get; set; }
        public string Author { get; set; }
        public string Img { get; set; }
        public string[] Urls { get; set; }
        public string[] UrlsHash { get; set; }
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

        GameObject m_ErrorPopUp;
        // i had to make this my fucking self since i dont have the WORKING BUNDLES FILES NEBULA KJDFJDHSJGHDSJKHGDSJG --triggered
        public GameObject ErrorPopUp { get {
            if(m_ErrorPopUp == null)
                {
                    Debugger.Log($"LevelPopUp pre {LevelPopUp} ------------------------------------------------------------------");
                    if (LevelPopUp == null)
                        LevelPopUp = GameObject.Find("Canvas/Main Menu (1)/EnvyScreen(Clone)/BrowseTab/LevelPopUp");
                    Debugger.Log($"LevelPopUp {LevelPopUp}");
                    m_ErrorPopUp = Instantiate(LevelPopUp);

                    m_ErrorPopUp.GetComponent<LevelPopUp>().enabled = false;
                    m_ErrorPopUp.GetComponent<HudOpenEffect>().enabled = false;
                    m_ErrorPopUp.GetComponent<UnityEngine.UI.Image>().color = new Color(1, 0, 0, 0.5f);

                    m_ErrorPopUp.transform.localScale = new Vector3(7.25f, 3, 0.8035f);
                    m_ErrorPopUp.transform.localPosition = new Vector3(0,0,0);
                    m_ErrorPopUp.transform.parent = GameObject.Find("Canvas/Main Menu (1)/EnvyScreen(Clone)/").transform;

                    m_ErrorPopUp.name = "ErrorPopUp";
                    GameObject.Find($"Canvas/Main Menu (1)/EnvyScreen(Clone)/{m_ErrorPopUp.name}/Panel/Title").GetComponent<TextMeshProUGUI>().text = "<size=32>An error has occurred.";
                    GameObject.Find($"Canvas/Main Menu (1)/EnvyScreen(Clone)/{m_ErrorPopUp.name}/Panel/Description").GetComponent<TextMeshProUGUI>().text = "Error here";
                    GameObject.Find($"Canvas/Main Menu (1)/EnvyScreen(Clone)/{m_ErrorPopUp.name}/Panel/Back/Text (TMP)").GetComponent<TextMeshProUGUI>().text = "OK";
                    m_ErrorPopUp.SetActive(false);
                }
            return m_ErrorPopUp;
            } set { m_ErrorPopUp = value; } } 
        public void PopupError(string error)
        {
            ErrorPopUp.SetActive(true);
            m_ErrorPopUp.transform.localScale = new Vector3(7.25f, 3, 0.8035f);
            m_ErrorPopUp.transform.localPosition = new Vector3(0, 0, 0);
            m_ErrorPopUp.transform.parent = GameObject.Find("Canvas/Main Menu (1)/EnvyScreen(Clone)/").transform;
            GameObject.Find($"Canvas/Main Menu (1)/EnvyScreen(Clone)/{m_ErrorPopUp.name}/Panel/Title").GetComponent<TextMeshProUGUI>().text = "<size=32>An error has occurred.";
            GameObject.Find($"Canvas/Main Menu (1)/EnvyScreen(Clone)/{m_ErrorPopUp.name}/Panel/Description").GetComponent<TextMeshProUGUI>().text = error;
            GameObject.Find($"Canvas/Main Menu (1)/EnvyScreen(Clone)/{m_ErrorPopUp.name}/Panel/Back/Text (TMP)").GetComponent<TextMeshProUGUI>().text = "OK";
        }

        public TMP_InputField SearchBar;

        [HideInInspector]
        public int Count;

        public static readonly string JsonURL = "https://raw.githubusercontent.com/SatisfiedBucket/EnvySpiteDownloader/main/Networking/EnvyDL.json";

        private string urlLoadResult;
        private byte[] urlImgLoadResult;

        public static readonly string tempPath = Path.Combine(System.IO.Path.GetTempPath(), "envydl");

        public void Refresh()
        {
            StartCoroutine(LoadURL());
        }

        public async void InstallButton(string[] DownloadURLS, string[] DownloadHASH, TextMeshProUGUI InstallText, string Name)
        {
            if(!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            else
            {
                foreach (FileInfo file in new DirectoryInfo(tempPath).GetFiles())
                {
                    if(!file.FullName.Contains(".doomah.part"))
                    {
                        Debugger.Log($"Skipping {file.Name} in {tempPath} because its not a doomah file!");
                    }    
                    Debugger.LogWarn($"Deleting {file.Name} in {tempPath}!");
                    file.Delete();
                }
            }

            Count = 0;
            foreach (string DownloadURL in DownloadURLS)
            {
                Count++;

                string[] splitURL = DownloadURL.Split('/');
                string orignalFileName = splitURL[splitURL.Length - 1];
                if (orignalFileName.Contains(".doomah.part"))
                    orignalFileName = Path.GetFileNameWithoutExtension(Path.GetFileNameWithoutExtension(orignalFileName)); // :crylq: --triggered
                else
                    orignalFileName = Path.GetFileNameWithoutExtension(orignalFileName);

                Debugger.Log($"Got url {DownloadURL} -> {orignalFileName}({splitURL[splitURL.Length - 1]}) with .doomah.part being {splitURL[splitURL.Length - 1].Contains(".doomah.part")}");

                string fileName = Path.Combine(tempPath, $"envydl_tmp_{orignalFileName}_{UnityEngine.Random.Range(-1024f, 1024f).ToString()}.doomah.part{Count}");
                string outFileName = Path.Combine(Plugin.getConfigPath(), orignalFileName + ".doomah.part" + Count);
                string outFileName_Normal = Path.Combine(Plugin.getConfigPath(), orignalFileName + ".doomah");

                if (File.Exists(outFileName) || File.Exists(outFileName_Normal))
                {
                    Debugger.LogWarn($"Level already downloaded dummy!\n{fileName} -> {outFileName} or {outFileName_Normal}");
                    InstallText.text = "Already installed!";
                    continue;
                }

                Task<bool> download_result = StartDownload(DownloadURL, fileName, InstallText, Count, DownloadURLS.Length, CancellationToken.None);
                await Task.Run(async () => {
                    while(!download_result.IsCompleted && !(download_result.IsFaulted || download_result.IsCanceled))
                        await Task.Delay(100);
                });

                if (download_result.Result && File.Exists(fileName))
                {
                    using (var md5 = MD5.Create())
                    {
                        using (var stream = File.OpenRead(fileName))
                        {
                            string Hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");
                            Debugger.Log($"Hash {Hash} == DownloadHASH {DownloadHASH[Count - 1]}");
                            if (Hash != DownloadHASH[Count - 1])
                            {
                                Debugger.LogError("Invalid hash, cancelling download.");
                                PopupError($"Invalid hash, cancelling download.\nGot {Hash} expected {DownloadHASH[Count - 1]}");
                                continue;
                            }
                        }
                        Debugger.Log($"Moving {fileName} to {outFileName}");
                        File.Move(fileName, outFileName);
                    }
                }
                else
                {
                    Debugger.LogError("Downloaded file does not exist!");
                    PopupError($"Downloaded file does not exist!\nURL: {DownloadURL}");
                    return;
                }
            }
            EnvyLoaderMenu.Instance.RefreshOnOpen = true;
        }

        //thanks chatgpt --triggered
        public static async Task<bool> StartDownload(string url, string name, TextMeshProUGUI InstallText, int count, int max, CancellationToken cancellationToken)
        {
            return await Task.Run(async () =>
            {
                // Create an HttpClient instance
                using HttpClient httpClient = new HttpClient();

                // Create TaskCompletionSource to signal download completion
                var tcs = new TaskCompletionSource<bool>();

                // Progress reporting
                long totalBytes = 0;
                var progressHandler = new Progress<long>(bytesReceived =>
                {
                    if(InstallText != null)
                        InstallText.text = $"{count}/{max}: {Mathf.FloorToInt((bytesReceived / 1000000.0f) * 10) / 10f}/{Mathf.FloorToInt((totalBytes / 1000000.0f) * 10) / 10f} MB";
                });

                Debugger.Log("writing to " + name);
                try
                {
                    // Send a GET request and download the file asynchronously
                    using (var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken))
                    {
                        response.EnsureSuccessStatusCode();

                        // Get the content length if available
                        totalBytes = response.Content.Headers.ContentLength ?? -1;

                        using (var fileStream = new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            using (var contentStream = await response.Content.ReadAsStreamAsync())
                            {
                                var buffer = new byte[8192];
                                int bytesRead;
                                long bytesDownloaded = 0;

                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                                {
                                    // File writing done in a background thread to prevent blocking the main thread
                                    await Task.Run(() => fileStream.WriteAsync(buffer, 0, bytesRead, cancellationToken));

                                    // Update progress
                                    bytesDownloaded += bytesRead;
                                    (progressHandler as IProgress<long>)?.Report(bytesDownloaded);
                                }
                            }
                        }

                        // Signal completion
                        tcs.SetResult(true);
                    }
                }
                catch (Exception ex)
                {
                    // Handle or log errors (e.g., download failure)
                    Debugger.LogError($"Download failed: {ex.Message}");
                    tcs.SetException(ex);
                    return false;
                }

                // Await download completion
                await tcs.Task;
                return true;
            });
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
                    Debugger.LogError("Failed to fetch Level List: " + webRequest.error);
                    urlLoadResult = null;
                }
                else
                {
                    urlLoadResult = webRequest.downloadHandler.text;
                }
            }

            string override_path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "envydl_override.json");
            Debugger.Log($"Checking for envydl override at {override_path}...");
            if(File.Exists(override_path))
                urlLoadResult = File.ReadAllText(override_path);

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

                TextMeshProUGUI installText = component.InstallButton.transform.Find("Text (TMP)").GetComponent<TextMeshProUGUI>();
                installText.fontSize = 8;
                installText.fontSizeMin = 8;
                installText.fontSizeMax = 8;

                component.InstallButton.onClick.AddListener(() => InstallButton(parameter.Urls, parameter.UrlsHash, installText, parameter.Name));
                component.AboutButton.onClick.AddListener(() => AboutButton(parameter.Name, parameter.Description));

                Texture2D levelImage = new Texture2D(2, 2);
                using (UnityWebRequest webRequest = UnityWebRequest.Get(parameter.Img))
                {
                    yield return webRequest.SendWebRequest();

                    if (webRequest.isNetworkError || webRequest.isHttpError)
                    {
                        Debugger.LogError("Failed to fetch Level List: " + webRequest.error);
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
                        Debugger.LogError($"Failed to load image for:" + parameter.Name);
                    }
                }
                else
                {
                    Debugger.LogWarn("Failed to load image, skipping");
                }
                Debugger.Log("Loaded " + parameter.Name + "!");
            }
        }
    }
}