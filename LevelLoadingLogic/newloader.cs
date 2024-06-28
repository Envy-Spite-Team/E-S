using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DoomahLevelLoader
{
    public static class LoaderScene
    {
        public static string LevelsPath => Plugin.getConfigPath();
        public static string UnpackedLevelsPath => Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        public static readonly List<AssetBundleInfo> AssetBundles = [];

        public static void LoadLevels() => Directory.GetFiles(LevelsPath, "*.doomah").Do(UnzipAndLoadBundles);

        public static void RefreshLevels()
        {
            AssetBundles.ForEach(bundle => bundle.Bundle.Unload(true));
            LoadLevels();
        }

        private static void UnzipAndLoadBundles(string doomahFile)
        {
            string extractPath = Path.Combine(UnpackedLevelsPath, Path.GetFileNameWithoutExtension(doomahFile));

            if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);
            ZipFile.ExtractToDirectory(doomahFile, extractPath);

            foreach (string bundleFile in Directory.GetFiles(extractPath, "*.bundle"))
            {
                AssetBundles.Add(new(bundleFile, extractPath));
            }
        }

        public static void LoadScene(LevelButtonScript buttonScript)
        {
            if (string.IsNullOrEmpty(buttonScript?.SceneToLoad)) return;
            SceneHelper.Instance.LoadSceneAsync(buttonScript.SceneToLoad, false);

            SceneHelper.ShowLoadingBlocker();
            SceneManager.LoadSceneAsync(buttonScript.SceneToLoad).completed += _ => SceneHelper.DismissBlockers();
        }

        public static void OpenFilesFolder() => Process.Start(Plugin.getConfigPath().Replace("\\", "/"));

        public static void SetLevelButtonScriptProperties(LevelButtonScript buttonScript, AssetBundleInfo bundleInfo)
        {
            buttonScript.BundleName = bundleInfo.Bundle;
            buttonScript.SceneToLoad = bundleInfo.ScenePaths.FirstOrDefault();
            buttonScript.OpenCamp = bundleInfo.IsCampaign;
            buttonScript.FileSize.text = bundleInfo.FileSize;

            buttonScript.Author.text = bundleInfo.Author ?? "Unknown";
            buttonScript.LevelName.text = bundleInfo.LevelNames.Any() ? bundleInfo.LevelNames.First() : "Unnamed";

            if (bundleInfo.LevelImages.Values.FirstOrDefault() is { } firstImage)
            {
                buttonScript.LevelImageButtonThing.sprite = Sprite.Create(firstImage, new Rect(0, 0, firstImage.width, firstImage.height), Vector2.zero);
            }

            buttonScript.NoLevel.gameObject.SetActive(!bundleInfo.LevelNames.Any());
            buttonScript.LevelImageButtonThing.gameObject.SetActive(bundleInfo.LevelNames.Any());
        }
    }

    public class AssetBundleInfo
    {
        public AssetBundle Bundle;
        public List<string> ScenePaths;
        public List<string> LevelNames;
        public Dictionary<string, Texture2D> LevelImages = [];
        public string FileSize;
        public string Author;
        public bool IsCampaign;

        public AssetBundleInfo(string bundleFile, string extractPath)
        {
            string infoPath = Path.Combine(extractPath, "info.json");
            LevelInfo levelInfo = !File.Exists(infoPath) ? null : new(infoPath);

            Bundle = AssetBundle.LoadFromFile(bundleFile);
            ScenePaths = new(levelInfo?.IsCampaign == true && levelInfo.Scenes != null ? levelInfo.Scenes : Bundle.GetAllScenePaths());
            LevelNames = levelInfo?.LevelNames;
            LevelImages = LoadLevelImages(levelInfo?.LevelImages);
            FileSize = GetFileSize();
            Author = levelInfo?.Author;
            IsCampaign = levelInfo?.IsCampaign ?? false;

            Dictionary<string, Texture2D> LoadLevelImages(List<string> imagePaths)
            {
                Dictionary<string, Texture2D> levelImages = [];
                if (imagePaths == null) return levelImages;

                foreach (string imagePath in imagePaths)
                {
                    string fullImagePath = Path.Combine(extractPath, imagePath);
                    if (!File.Exists(fullImagePath)) continue;

                    byte[] imageBytes = File.ReadAllBytes(fullImagePath);
                    Texture2D levelImage = new(2, 2);
                    levelImage.LoadImage(imageBytes);
                    levelImages[imagePath] = levelImage;
                }

                return levelImages;
            }

            // https://stackoverflow.com/a/11124118
            string GetFileSize()
            {
                long value = new FileInfo(bundleFile).Length;
                string suffix;
                double readable;
                switch (Math.Abs(value))
                {
                    case >= 0x1000000000000000:
                        suffix = "EiB";
                        readable = value >> 50;
                        break;
                    case >= 0x4000000000000:
                        suffix = "PiB";
                        readable = value >> 40;
                        break;
                    case >= 0x10000000000:
                        suffix = "TiB";
                        readable = value >> 30;
                        break;
                    case >= 0x40000000:
                        suffix = "GiB";
                        readable = value >> 20;
                        break;
                    case >= 0x100000:
                        suffix = "MiB";
                        readable = value >> 10;
                        break;
                    case >= 0x400:
                        suffix = "KiB";
                        readable = value;
                        break;
                    default:
                        return value.ToString("0 B");
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
        public List<string> Scenes = [];
        public List<string> LevelNames = [];
        public List<string> LevelImages = [];

        public LevelInfo(string path) => JsonUtility.FromJson<LevelInfo>(File.ReadAllText(path));
    }
}
