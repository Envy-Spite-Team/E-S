using System;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using BepInEx;
using UnityEngine.UI;
using TMPro;

namespace DoomahLevelLoader
{
    public static class Loaderscene
    {
        public static List<AssetBundle> loadedAssetBundles = new List<AssetBundle>();
        public static int currentAssetBundleIndex = 0;
        public static List<string> bundleFolderPaths = new List<string>();
        private static EnvyLoaderMenu envyLoaderMenuScript;

        public static string LoadedSceneName { get; set; }
        private static string executablePath;
        private static string directoryPath;

        static Loaderscene()
        {
            executablePath = Assembly.GetExecutingAssembly().Location;
            directoryPath = Path.GetDirectoryName(executablePath);
        }

        public static string GetUnpackedLevelsPath()
        {
            return Path.Combine(directoryPath, "UnpackedLevels");
        }

        public static async Task Setup()
        {
            await RecreateUnpackedLevelsFolder(GetUnpackedLevelsPath());
        }

        public static async Task RecreateUnpackedLevelsFolder(string unpackedLevelsPath)
        {
            if (Directory.Exists(unpackedLevelsPath))
            {
                Directory.Delete(unpackedLevelsPath, true);
            }
            Directory.CreateDirectory(unpackedLevelsPath);
            string[] doomahFiles = Directory.GetFiles(Plugin.getConfigPath(), "*.doomah");
            foreach (string doomahFile in doomahFiles)
            {
                string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(doomahFile);
                string levelFolderPath = Path.Combine(unpackedLevelsPath, fileNameWithoutExtension);

                try
                {
                    await Task.Run(() => ZipFile.ExtractToDirectory(doomahFile, levelFolderPath));
                    _ = LoadAssetBundle(levelFolderPath);
                }
                catch
                {
                    string fileName = Path.GetFileName(doomahFile);
                    UnityEngine.Debug.LogError($"Failed to extract {fileName}! Please uninstall map or ask creator to update!");
                }
            }
            EnvyLoaderMenu.UpdateLevelListing();
            await Task.CompletedTask;
        }

        public static async Task Refresh()
        {
            EnvyandSpiteterimal envyScript = GameObject.FindObjectOfType<EnvyandSpiteterimal>();
            if (envyScript != null)
            {
                envyScript.FuckingPleaseWait.SetActive(true);
            }
            else
            {
                envyLoaderMenuScript = GameObject.FindObjectOfType<EnvyLoaderMenu>();
                if (envyLoaderMenuScript != null)
                {
                    envyLoaderMenuScript.FuckingPleaseWait.SetActive(true);
                }
            }

            string unpackedLevelsPath = GetUnpackedLevelsPath();
            await RecreateUnpackedLevelsFolder(unpackedLevelsPath);

            List<Task> setupTasks = new List<Task>
           {
           Setup()
            };

            await Task.WhenAll(setupTasks);

            if (envyScript != null)
            {
                envyScript.FuckingPleaseWait.SetActive(false);
            }
            else if (envyLoaderMenuScript != null)
            {
                envyLoaderMenuScript.FuckingPleaseWait.SetActive(false);
            }
            EnvyLoaderMenu.UpdateLevelListing();
        }


        public static async Task LoadAssetBundle(string folderPath)
        {
            string[] bundleFiles = Directory.GetFiles(folderPath, "*.bundle");

            foreach (string bundleFile in bundleFiles)
            {
                await Task.Run(() =>
                {
                    AssetBundle assetBundle = AssetBundle.LoadFromFile(bundleFile);
                    if (assetBundle != null)
                    {
                        loadedAssetBundles.Add(assetBundle);
                        bundleFolderPaths.Add(Path.GetDirectoryName(bundleFile));
                    }
                });
            }
        }

        public static string GetCurrentBundleFolderPath()
        {
            if (currentAssetBundleIndex >= 0 && currentAssetBundleIndex < bundleFolderPaths.Count)
            {
                return bundleFolderPaths[currentAssetBundleIndex];
            }
            return null;
        }

        public static void SelectAssetBundle(int index)
        {
            if (index >= 0 && index < loadedAssetBundles.Count)
            {
                currentAssetBundleIndex = index;
            }
        }

        public static void ExtractSceneName()
        {
            if (loadedAssetBundles.Count > 0)
            {
                string[] scenePaths = loadedAssetBundles[currentAssetBundleIndex].GetAllScenePaths();
                if (scenePaths.Length > 0)
                {
                    string sceneName = Path.GetFileNameWithoutExtension(scenePaths[0]);
                    LoadedSceneName = sceneName;
                }
            }
        }

        public static void Loadscene()
        {
            if (!string.IsNullOrEmpty(LoadedSceneName))
            {
                SceneManager.LoadSceneAsync(LoadedSceneName).completed += OnSceneLoadComplete;
                SceneHelper.ShowLoadingBlocker();
            }
        }

        private static void OnSceneLoadComplete(AsyncOperation asyncOperation)
        {
            SceneHelper.DismissBlockers();
        }

        public static void MoveToNextAssetBundle()
        {
            currentAssetBundleIndex = (currentAssetBundleIndex + 1) % loadedAssetBundles.Count;
        }

        public static void MoveToPreviousAssetBundle()
        {
            currentAssetBundleIndex = (currentAssetBundleIndex - 1 + loadedAssetBundles.Count) % loadedAssetBundles.Count;
        }

        public static void OpenFilesFolder()
        {
            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.WindowsPlayer:
                    Application.OpenURL("file:///" + Plugin.getConfigPath().Replace("\\", "/"));
                    break;
                case RuntimePlatform.OSXEditor:
                case RuntimePlatform.OSXPlayer:
                    Application.OpenURL("file://" + Plugin.getConfigPath());
                    break;
                case RuntimePlatform.LinuxEditor:
                case RuntimePlatform.LinuxPlayer:
                    Application.OpenURL("file://" + Plugin.getConfigPath());
                    break;
                default:
                    UnityEngine.Debug.LogWarning("BROTHER WHAT IS YOUR OS?????");
                    break;
            }
        }

        public static void UpdateLevelPicture(Image levelPicture, TextMeshProUGUI frownyFace, bool getFirstBundle = true, string bundlePath = "")
        {
            try
            {
                string bundleFolderPath = "";

                if (getFirstBundle)
                {
                    bundleFolderPath = GetCurrentBundleFolderPath();
                }
                else
                {
                    if (!string.IsNullOrEmpty(bundlePath))
                    {
                        bundleFolderPath = bundlePath;
                    }
                    else
                    {
                        UnityEngine.Debug.LogError("Bundle folder path is null or empty.");
                        frownyFace.gameObject.SetActive(true);
                        levelPicture.color = new Color(1f, 1f, 1f, 0f);
                        return; // Early return to prevent further execution
                    }
                }

                if (!string.IsNullOrEmpty(bundleFolderPath))
                {
                    string[] imageFiles = Directory.GetFiles(bundleFolderPath, "*.png");
                    if (imageFiles.Length > 0)
                    {
                        string imagePath = imageFiles[0];
                        Texture2D tex = LoadTextureFromFile(imagePath);
                        if (tex != null)
                        {
                            tex.filterMode = FilterMode.Point;
                            levelPicture.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), Vector2.zero);
                            frownyFace.gameObject.SetActive(false);
                            levelPicture.color = Color.white;
                        }
                    }
                    else
                    {
                        frownyFace.gameObject.SetActive(true);
                        levelPicture.color = new Color(1f, 1f, 1f, 0f);
                    }
                }
                else
                {
                    UnityEngine.Debug.LogError("Bundle folder path is null or empty.");
                    frownyFace.gameObject.SetActive(true);
                    levelPicture.color = new Color(1f, 1f, 1f, 0f);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"An error occurred while updating level picture: {ex.Message}");
                frownyFace.gameObject.SetActive(true);
                levelPicture.color = new Color(1f, 1f, 1f, 0f);
            }
        }

        private static Texture2D LoadTextureFromFile(string path)
        {
            byte[] fileData = File.ReadAllBytes(path);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            return texture;
        }

        public static string GetAssetBundleSize(int index)
        {
            if (index >= 0 && index < loadedAssetBundles.Count)
            {
                AssetBundle assetBundle = loadedAssetBundles[index];
                string bundlePath = bundleFolderPaths[index];
                long fileSize = CalculateFileSize(bundlePath);
                string fileSizeFormatted = FormatFileSize(fileSize);
                return fileSizeFormatted;
            }
            else
            {
                return "Index out of range";
            }
        }

        private static long CalculateFileSize(string bundlePath)
        {
            long totalSize = 0;
            string[] files = Directory.GetFiles(bundlePath, "*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                totalSize += new FileInfo(file).Length;
            }
            return totalSize;
        }

        private static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB", "PB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{Math.Round(size, 2)} {suffixes[suffixIndex]}";
        }
    }
}
