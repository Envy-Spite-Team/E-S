using Mono.Security.Cryptography;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace DoomahLevelLoader
{
    public class EnvyDownloaderMenu_DEBUG : MonoBehaviour
    {
        public GameObject menuButton;
        public GameObject levelsView;
        public GameObject levelTemplate;
        public GameObject editPopup;
        public EnvyDownloaderEditMenu_DEBUG editPopup_menu;

        public void Start()
        {
            levelsView = transform.Find("Scroll View").Find("Viewport").Find("Content").gameObject;
            levelTemplate = levelsView.transform.Find("level_inst").gameObject;
            editPopup = transform.Find("edit-popup").gameObject;

            levelTemplate.SetActive(false);
            editPopup_menu = editPopup.AddComponent<EnvyDownloaderEditMenu_DEBUG>();
            editPopup_menu.preview = editPopup.transform.Find("preview_level").gameObject.AddComponent<EnvyDownloaderLevel_DEUBG>();
            RefreshLevelsProxy();
        }

        public void RefreshLevelsProxy()
        {
            StartCoroutine(RefreshLevels());
        }

        public IEnumerator RefreshLevels()
        {
            string urlLoadResult = "";
            byte[] urlImgLoadResult = new byte[] { 0 };

            using (UnityWebRequest webRequest = UnityWebRequest.Get(EnvyDownloaderMenu.JsonURL))
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
            if (File.Exists(override_path))
                urlLoadResult = File.ReadAllText(override_path);

            List<Parameter> parameters = JsonConvert.DeserializeObject<List<Parameter>>(urlLoadResult);

            foreach (Transform child in levelsView.transform)
            {
                if(child.gameObject == levelTemplate)
                    continue;
                Destroy(child.gameObject);
            }

            foreach (var parameter in parameters)
            {
                GameObject newLevel = Instantiate(levelTemplate);

                newLevel.transform.SetParent(levelsView.transform, false);
                EnvyDownloaderLevel_DEUBG data = newLevel.AddComponent<EnvyDownloaderLevel_DEUBG>();
                data.GetObjects();

                data.lvlName.text = parameter.Name;
                data.lvlAuth.text = parameter.Author;
                newLevel.SetActive(true);
                newLevel.GetComponentInChildren<Button>().onClick.AddListener(() => {
                    editPopup.SetActive(true);
                    editPopup_menu.target = data;
                });
            }
        }
    }
    public class EnvyDownloaderEditMenu_DEBUG : MonoBehaviour
    {
        public TMP_InputField lvlName;
        public TMP_InputField lvlAuth;
        public TMP_InputField lvlDesc;

        public TMP_InputField lvlVers;
        public Toggle lvlSati;

        public TMP_InputField imgLink;

        public TMP_InputField file1;
        public TMP_InputField file2;
        public TMP_InputField file3;
        public TMP_InputField file4;

        public EnvyDownloaderLevel_DEUBG target;
        public EnvyDownloaderLevel_DEUBG preview;
        public TextMeshProUGUI desc_preview;

        public Button save;
        public Button cancel;

        public Parameter levelData;

        public void Awake()
        {
            lvlName = transform.Find("name").GetComponent<TMP_InputField>();
            lvlAuth = transform.Find("auth").GetComponent<TMP_InputField>();
            lvlDesc = transform.Find("desc").GetComponent<TMP_InputField>();

            lvlVers = transform.Find("version").GetComponent<TMP_InputField>();
            lvlSati = transform.Find("Toggle").GetComponent<Toggle>();

            imgLink = transform.Find("image_link").GetComponent<TMP_InputField>();
            file1 = transform.Find("file 1").GetComponent<TMP_InputField>();
            file2 = transform.Find("file 2").GetComponent<TMP_InputField>();
            file3 = transform.Find("file 3").GetComponent<TMP_InputField>();
            file4 = transform.Find("file 4").GetComponent<TMP_InputField>();

            desc_preview = transform.Find("preview_desc_box").Find("text").GetComponent<TextMeshProUGUI>();

            save = transform.Find("save").gameObject.GetComponent<Button>();
            cancel = transform.Find("cancel").gameObject.GetComponent<Button>();

            cancel.onClick.AddListener(() => {
                gameObject.SetActive(false);
            });

            save.onClick.AddListener(async () => {
                if (target != null)
                {
                    if (target.lvlName == null)
                        target.GetObjects();

                    target.lvlName.text = lvlName.text;
                    target.lvlAuth.text = lvlAuth.text;
                    if (target.lastUrl != imgLink.text)
                        target.LoadImageProxy(imgLink.text);
                }

                levelData = new Parameter();
                levelData.Name = lvlName.text;
                levelData.Author = lvlAuth.text;
                levelData.Description = lvlDesc.text;
                levelData.Version = lvlVers.text;
                levelData.Serious = !lvlSati.isOn;
                levelData.Img = imgLink.text;

                List<string> files = new List<string>();

                if (!string.IsNullOrEmpty(file1.text))
                    files.Add(file1.text);
                if (!string.IsNullOrEmpty(file2.text))
                    files.Add(file2.text);
                if (!string.IsNullOrEmpty(file3.text))
                    files.Add(file3.text);
                if (!string.IsNullOrEmpty(file4.text))
                    files.Add(file4.text);

                levelData.Urls = files.ToArray();

                string base_url = EnvyDownloaderMenu.tempPath;
                if (!Directory.Exists(base_url))
                    Directory.CreateDirectory(base_url);

                foreach (FileInfo file in new DirectoryInfo(base_url).GetFiles())
                {
                    if (!file.FullName.Contains(".unknown"))
                    {
                        Debugger.Log($"Skipping {file.Name} in {base_url} because its not a .unknown file!");
                    }
                    Debugger.LogWarn($"Deleting {file.Name} in {base_url}!");
                    file.Delete();
                }

                List<string> hashes = new List<string>();
                foreach (string url in levelData.Urls)
                {
                    string fileName = $"envydl_tmp_hashdebug_{UnityEngine.Random.Range(-1024f, 1024f).ToString()}.unknown";
                    Debugger.Log($"Downloading {fileName}");

                    string THISFUCKINGPATH = Path.Combine(base_url, fileName); // i was using fileName instead of this variable i just made --dumbass triggered

                    Task<bool> download_result = EnvyDownloaderMenu.StartDownload(url, THISFUCKINGPATH, null, 1, 1, CancellationToken.None);
                    await Task.Run(async () => {
                        while (!download_result.IsCompleted && !(download_result.IsFaulted || download_result.IsCanceled))
                            await Task.Delay(100);
                    });
                    Debugger.Log($"Done downloading {fileName}");
                    if (File.Exists(THISFUCKINGPATH))
                    {
                        using (var md5 = MD5.Create())
                        {
                            using (var stream = File.OpenRead(THISFUCKINGPATH))
                            {
                                string Hash = BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "");
                                Debugger.Log($"Got hash {Hash} for {fileName}");
                                hashes.Add(Hash);
                            }
                        }
                    }
                    else
                    {
                        Debugger.LogError("Failed to get " + fileName);
                    }
                }
                levelData.UrlsHash = hashes.ToArray();
                Debugger.Log($"levelData.UrlsHash {levelData.UrlsHash} Length of {levelData.UrlsHash.Length}");

                string json_out = JsonConvert.SerializeObject(levelData);
                Debugger.Log("Got json_out as " + json_out);
                FindObjectOfType<EnvyDownloaderMenu_DEBUG>().transform.Find("output").gameObject.GetComponent<TMP_InputField>().text = json_out;
            });
        }

        void OnEnable()
        {
            Debugger.Log(lvlName);
            Debugger.Log(target.lvlName);
            lvlName.text = target.lvlName.text;
            lvlAuth.text = target.lvlAuth.text;
        }

        void Update()
        {
            if (preview != null)
            {
                if (preview.lvlName == null)
                    preview.GetObjects();

                preview.lvlName.text = lvlName.text;
                preview.lvlAuth.text = lvlAuth.text;
                if (preview.lastUrl != imgLink.text)
                    preview.LoadImageProxy(imgLink.text);
            }

            if(desc_preview != null)
                desc_preview.text = lvlDesc.text;
        }
    }
    public class EnvyDownloaderLevel_DEUBG : MonoBehaviour
    {
        public TextMeshProUGUI lvlName;
        public TextMeshProUGUI lvlAuth;

        public Image            lvlImg;
        public string           lastUrl { get; protected set; }

        public void GetObjects()
        {
            lvlName = transform.Find("Level name").gameObject.GetComponent<TextMeshProUGUI>();
            lvlAuth = transform.Find("Author").gameObject.GetComponent<TextMeshProUGUI>();

            lvlImg  = transform.Find("Level Image").gameObject.GetComponent<Image>();
        }

        public void LoadImageProxy(string url)
        {
            StartCoroutine(LoadImage(url));
        }

        public IEnumerator LoadImage(string url)
        {
            lvlImg.transform.Find("Image").gameObject.SetActive(true);
            lvlImg.transform.Find("No level").gameObject.SetActive(true);

            lastUrl = url;
            string urlLoadResult = "";
            byte[] urlImgLoadResult = new byte[] {0};

            Texture2D levelImage = new Texture2D(2, 2);
            using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.isNetworkError || webRequest.isHttpError)
                {
                    Debugger.LogError("Failed to fetch level image: " + webRequest.error + "\nAt: "+url);
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
                    Debugger.Log($"Loaded image for: {url} successfully!");
                    lvlImg.sprite = Sprite.Create(levelImage, new Rect(0, 0, levelImage.width, levelImage.height), Vector2.zero);
                    lvlImg.color = Color.white;
                    lvlImg.transform.Find("Image").gameObject.SetActive(false);
                    lvlImg.transform.Find("No level").gameObject.SetActive(false);
                }
                else
                {
                    Debugger.LogError($"Failed to load image for:" + url);
                }
            }
            else
            {
                Debugger.LogWarn("Failed to load image, skipping");
            }
        }
    }
}
