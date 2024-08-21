using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Yoga;

namespace DoomahLevelLoader
{
    public static class Loaderscene
    {
        public static string LevelsPath => Plugin.getConfigPath();
        public static string currentLevelName = null;
        public static string currentLevelpath = null;
        public static List<AssetBundleInfo> AssetBundles = new List<AssetBundleInfo>();

        public static async Task LoadLevels()
        {
            // lebron james reportedly caught trying to invoke a null variable --thebluenebula
            // fuck onLevelsLoaded, coconut.jpeg my ass --triggered

            string[] files = Directory.GetFiles(LevelsPath, "*.doomah");

            GameObject myBlocker = GameObject.Instantiate(MonoSingleton<SceneHelper>.Instance.loadingBlocker, MonoSingleton<SceneHelper>.Instance.loadingBlocker.transform.parent);
            TextMeshProUGUI text = myBlocker.GetComponentInChildren<TextMeshProUGUI>();
            text.text = "Loading levels...\n<size=60><b>0/" + files.Length.ToString() + "</b></size>";
            myBlocker.SetActive(true);

            loadProgress = 0;
            loadProgressMax = files.Length;
            int fileIndex = 1;
            foreach (string file in files)
            {
                try
                {
                    _ = LoadBundlesFromDoomah(file, fileIndex / 2, text); // SHUT THE FUCK UP!!!!! :fire: --thebluenebula
                    // i made it shut the fuck up lmao --triggered
                    UnityEngine.Debug.Log(file + " loaded!");
                    await Task.Delay(16);
                }
                catch (Exception e) { UnityEngine.Debug.LogError(file + " failed to load: " + e.ToString()); }

                fileIndex++;
            }

            GameObject.Destroy(myBlocker);
            EnvyLoaderMenu.UpdateLevelListing();
        }
        static short loadProgress = 0;
        static int loadProgressMax = 0;
        static async Task LoadBundlesFromDoomah(string doomahFile, int index, TextMeshProUGUI blocker)
        {
            await Task.Delay(16); //big wait to let ShowLoadingBlocker render

            using (ZipArchive archive = ZipFile.OpenRead(doomahFile))
            {
                foreach (var entry in archive.Entries)
                {
                    if (entry.FullName.EndsWith(".bundle", StringComparison.OrdinalIgnoreCase))
                    {
                        Stream s = entry.Open();
                        byte[] bundleBytes;
                        using (var memstream = new MemoryStream())
                        {
                            new StreamReader(s).BaseStream.CopyTo(memstream);
                            bundleBytes = memstream.ToArray();
                        }

                        AssetBundles.Add(new AssetBundleInfo(bundleBytes, archive));

                        loadProgress++;
                        blocker.text = "Loading levels...\n<size=60><b>" + loadProgress.ToString() + "/" + loadProgressMax.ToString() + "</b></size>";
                    }
                }
            }
        }

        public static async void RefreshLevels()
        {
            AssetBundles = new List<AssetBundleInfo>();
            await Merger.MergeFiles();
            await LoadLevels();
        }

        public static byte[] ReadFully(Stream input)
        {
            using (BinaryReader reader = new BinaryReader(input))
            {
                byte[] allBytes = reader.ReadBytes((int)input.Length);
                return allBytes;
            }
        }

        public static AssetBundle lastUsedBundle { get; private set; }
        public static bool isLoadingScene { get; private set; } // you can spam click button so i fixed that :D --triggered
        public static void LoadScene(LevelButtonScript buttonScript)
        {
            if (isLoadingScene) return;
            isLoadingScene = true;

            SceneHelper.ShowLoadingBlocker();

            if (lastUsedBundle) lastUsedBundle.Unload(true);

            lastUsedBundle = AssetBundle.LoadFromMemory(buttonScript?.BundleDataToLoad);
            buttonScript.BundleName = lastUsedBundle;
            buttonScript.SceneToLoad = lastUsedBundle.GetAllScenePaths().FirstOrDefault();

            if (string.IsNullOrEmpty(buttonScript?.SceneToLoad)) return;

            SceneHelper.Instance.LoadSceneAsync(buttonScript.SceneToLoad, false);
            currentLevelpath = buttonScript.SceneToLoad;
            currentLevelName = buttonScript.LevelName.text;

            SceneManager.LoadSceneAsync(buttonScript.SceneToLoad).completed += operation =>
            {
                SceneHelper.DismissBlockers();
                Plugin.Fixorsmth();
                isLoadingScene = false;
            };
        }

        public static void OpenFilesFolder() => Application.OpenURL("file://" + Plugin.getConfigPath().Replace("\\", "/"));

        public static void SetLevelButtonScriptProperties(LevelButtonScript buttonScript, AssetBundleInfo bundleInfo)
        {
            buttonScript.BundleName = bundleInfo.Bundle;
            buttonScript.BundleDataToLoad = bundleInfo.BundleDataToLoad;
            buttonScript.SceneToLoad = "";
            buttonScript.OpenCamp = bundleInfo.IsCampaign;
            //buttonScript.FileSize.text = bundleInfo.FileSize;

            buttonScript.Author.text = bundleInfo.Author ?? "Unknown";
            buttonScript.LevelName.text = bundleInfo.LevelNames.Any() ? bundleInfo.LevelNames.First() : "Unnamed";
            if (bundleInfo.LevelImages.Values.FirstOrDefault() is Texture2D firstImage)
            {
                if (firstImage.width > 2 && firstImage.height > 2)
                {
                    buttonScript.LevelImageButtonThing.sprite = Sprite.Create(firstImage, new Rect(0, 0, firstImage.width, firstImage.height), Vector2.zero);
                    buttonScript.LevelImageButtonThing.color = Color.white;
                }
                else
                {
                    UnityEngine.Debug.LogError("Failed to load level image properly.");
                    buttonScript.LevelImageButtonThing.color = Color.clear;
                }
            }
            else
            {
                buttonScript.LevelImageButtonThing.color = Color.clear;
            }

            bool hasLevelName = bundleInfo.LevelNames.Any();
            buttonScript.NoLevel.gameObject.SetActive(!hasLevelName);
            buttonScript.LevelImageButtonThing.gameObject.SetActive(hasLevelName);
        }
        public static bool IsSceneInAnyAssetBundle(string sceneName)
        {
            if (lastUsedBundle)
            {
                foreach (string path in lastUsedBundle.GetAllScenePaths())
                    if (Path.GetFileNameWithoutExtension(path) == sceneName) return true;
            }
            return false;
        }
    }

    public class AssetBundleInfo
    {
        public AssetBundle Bundle;
        public byte[] BundleDataToLoad;
        public List<string> ScenePaths;
        public List<string> LevelNames;
        public Dictionary<string, Texture2D> LevelImages;
        public string FileSize;
        public string Author;
        public bool IsCampaign;

        public AssetBundleInfo(byte[] bundleData, ZipArchive archive)
        {
            BundleDataToLoad = bundleData;
            ZipArchiveEntry infoJsonEntry = archive.GetEntry("info.json");
            ZipArchiveEntry infoTxtEntry = archive.GetEntry("info.txt");

            LevelInfo levelInfo = null;

            if (infoJsonEntry != null)
            {
                using (var reader = new StreamReader(infoJsonEntry.Open()))
                {
                    levelInfo = LevelInfo.LoadFromJson(reader.ReadToEnd());
                }
            }
            else if (infoTxtEntry != null)
            {
                using (var reader = new StreamReader(infoTxtEntry.Open()))
                {
                    levelInfo = LevelInfo.LoadFromText(reader.ReadToEnd());
                }
            }

            IsCampaign = levelInfo?.IsCampaign ?? false;
            LevelNames = levelInfo?.LevelNames ?? new List<string>();

            // fix missing level images
            levelInfo.LevelImages = new List<string>();
            foreach (ZipArchiveEntry file in archive.Entries)
            {
                if (Path.GetExtension(file.FullName) == ".png") levelInfo?.LevelImages.Add(file.FullName);
            }

            LevelImages = LoadLevelImages(levelInfo?.LevelImages, archive);
            //FileSize = GetFileSize(bundleData.Length);
            Author = levelInfo?.Author;

            Dictionary<string, Texture2D> LoadLevelImages(List<string> imagePaths, ZipArchive zipArchive)
            {
                Dictionary<string, Texture2D> levelImages = new Dictionary<string, Texture2D>();
                if (imagePaths == null) return levelImages;

                foreach (string imagePath in imagePaths)
                {
                    var imageEntry = zipArchive.GetEntry(imagePath);
                    if (imageEntry == null) continue;

                    using (var reader = new MemoryStream())
                    {
                        imageEntry.Open().CopyTo(reader);
                        byte[] imageBytes = reader.ToArray();

                        Texture2D levelImage = new Texture2D(2, 2);
                        if (levelImage.LoadImage(imageBytes))
                        {
                            levelImages[imagePath] = levelImage;
                        }
                        else
                        {
                            UnityEngine.Debug.LogError($"Failed to load image from path: {imagePath}");
                        }
                    }
                }

                return levelImages;
            }

            string GetFileSize(long length)
            {
                string suffix;
                double readable;

                switch (length)
                {
                    case >= 0x1000000000000000:
                        suffix = "EiB";
                        readable = length >> 50;
                        break;
                    case >= 0x4000000000000:
                        suffix = "PiB";
                        readable = length >> 40;
                        break;
                    case >= 0x10000000000:
                        suffix = "TiB";
                        readable = length >> 30;
                        break;
                    case >= 0x40000000:
                        suffix = "GiB";
                        readable = length >> 20;
                        break;
                    case >= 0x100000:
                        suffix = "MiB";
                        readable = length >> 10;
                        break;
                    case >= 0x400:
                        suffix = "KiB";
                        readable = length;
                        break;
                    default:
                        return length.ToString("0 B");
                }

                return (readable / 1024).ToString("0.## ") + suffix;
            }
        }
    }

    [Serializable]
    public class LevelInfo
    {
        public string Author;
        public string LevelName;
        public bool IsCampaign;
        public List<string> Scenes = new List<string>();
        public List<string> LevelNames = new List<string>();
        public List<string> LevelImages = new List<string>();

        public static LevelInfo LoadFromJson(string json) => JsonUtility.FromJson<LevelInfo>(json);

        public static LevelInfo LoadFromText(string text)
        {
            LevelInfo info = new LevelInfo();
            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0) info.Author = lines[0].Trim();
            if (lines.Length > 1) info.LevelNames.Add(lines[1].Trim());
            info.IsCampaign = false;
            return info;
        }
    }
}
