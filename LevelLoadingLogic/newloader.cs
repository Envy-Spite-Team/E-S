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

namespace DoomahLevelLoader:

public static class LoaderScene
{
	public static string LevelsPath { get; } = Plugin.getConfigPath();
	public static string UnpackedLevelsPath { get; } = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
	public static List<AssetBundleInfo> AssetBundles { get; } = new List<AssetBundleInfo>();

	// Load all level files and unzip them asynchronously
	public static async Task LoadLevels()
	{
		var doomahFiles = Directory.GetFiles(LevelsPath, "*.doomah");
		var unzipTasks = new List<Task>();

		// Create a task for each file to unzip and load
		foreach (var file in doomahFiles)
		{
			unzipTasks.Add(UnzipAndLoadBundles(file));
		}

		await Task.WhenAll(unzipTasks);
	}

	// Unload all asset bundles and reload levels
	public static async Task RefreshLevels()
	{
		AssetBundles.ForEach(bundle => bundle.Bundle.Unload(true));
		await LoadLevels();
	}

	// Unzip the file and load the asset bundles
    private static async Task UnzipAndLoadBundles(string doomahFile)
    {
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(doomahFile);
        var extractPath = Path.Combine(UnpackedLevelsPath, fileNameWithoutExtension);

        // Calculate file size
        var fileSize = CalculateFileSize(doomahFile);

        if (Directory.Exists(extractPath)) Directory.Delete(extractPath, true);

        ZipFile.ExtractToDirectory(doomahFile, extractPath);

        // Load level information from info.json
        var levelInfo = LoadLevelInfo(Path.Combine(extractPath, "info.json"));
        var levelImages = LoadLevelImages(levelInfo?.LevelImages, extractPath);

        foreach (var bundleFile in Directory.GetFiles(extractPath, "*.bundle"))
        {
            var bundle = AssetBundle.LoadFromFile(bundleFile);
            if (bundle == null) continue;

            // Determine scene paths based on whether the level is a campaign
            var scenePaths = levelInfo?.IsCampaign == true && levelInfo.Scenes != null
                ? new List<string>(levelInfo.Scenes)
                : new List<string>(bundle.GetAllScenePaths());

            AssetBundles.Add(new AssetBundleInfo
            {
                Bundle = bundle,
                ScenePaths = scenePaths,
                LevelNames = levelInfo?.LevelNames,
                LevelImages = levelImages,
                FileSize = fileSize,
                Author = levelInfo?.Author,
                IsCampaign = levelInfo?.IsCampaign ?? false
            });
        }
    }

	// Method to calculate file size
	private static float CalculateFileSize(string filePath)
	{
		FileInfo fileInfo = new FileInfo(filePath);
		double bytes = fileInfo.Length;

		string[] suffix = { "B", "KB", "MB", "GB", "TB" };

		for (int i = 0; i < suffix.Length - 1 && bytes >= 1024; i++)
		{
			bytes /= 1024;
		}

		return (float)Math.Round(bytes, 2);
	}

	// Load level information from the info.json file
    private static LevelInfo LoadLevelInfo(string infoJsonPath)
    {
        if (!File.Exists(infoJsonPath)) return null;
        var json = File.ReadAllText(infoJsonPath);
        var levelInfo = JsonUtility.FromJson<LevelInfo>(json);
        return levelInfo;
    }

	// Load level images from the specified paths
	private static Dictionary<string, Texture2D> LoadLevelImages(List<string> imagePaths, string extractPath)
	{
		var levelImages = new Dictionary<string, Texture2D>();
		if (imagePaths == null) return levelImages;

		foreach (var imagePath in imagePaths)
		{
			var fullImagePath = Path.Combine(extractPath, imagePath);
			if (!File.Exists(fullImagePath)) continue;

			var imageBytes = File.ReadAllBytes(fullImagePath);
			var levelImage = new Texture2D(2, 2);
			levelImage.LoadImage(imageBytes);
			levelImages[imagePath] = levelImage;
		}

		return levelImages;
	}
	
	public static void Loadscene(LevelButtonScript buttonScript)
	{
		if (buttonScript == null || string.IsNullOrEmpty(buttonScript.SceneToLoad)) return;

		SceneManager.LoadSceneAsync(buttonScript.SceneToLoad).completed += asyncOperation => SceneHelper.DismissBlockers();
		SceneHelper.ShowLoadingBlocker();
	}
	
	public static void OpenFilesFolder()
	{
		string configPath = Plugin.getConfigPath().Replace("\\", "/");
		string url = "file://";

		switch (Application.platform)
		{
			case RuntimePlatform.WindowsPlayer:
				url += "/" + configPath;
				break;
			case RuntimePlatform.OSXPlayer:
			case RuntimePlatform.LinuxPlayer:
				url += configPath;
				break;
			default:
				UnityEngine.Debug.LogWarning("BROTHER WHAT IS YOUR OS?????");
				return;
		}
		Application.OpenURL(url);
	}
	
	public static void SetLevelButtonScriptProperties(LevelButtonScript buttonScript, AssetBundleInfo bundleInfo)
	{
		buttonScript.BundleName = bundleInfo.Bundle;
		buttonScript.SceneToLoad = bundleInfo.ScenePaths.Count > 0 ? bundleInfo.ScenePaths[0] : null;
		buttonScript.OpenCamp = bundleInfo.IsCampaign;
		buttonScript.FileSize.text = $"{bundleInfo.FileSize} MB";
		
		buttonScript.Author.text = bundleInfo.Author ?? "Unknown";
		buttonScript.LevelName.text = bundleInfo.LevelNames.Count > 0 ? bundleInfo.LevelNames[0] : "Unnamed";
		
		if (bundleInfo.LevelImages.Count > 0)
		{
			var firstImage = bundleInfo.LevelImages.Values.FirstOrDefault();
			if (firstImage != null)
			{
				buttonScript.LevelImageButtonThing.sprite = Sprite.Create(firstImage, new Rect(0, 0, firstImage.width, firstImage.height), Vector2.zero);
			}
		}
		
		buttonScript.NoLevel.gameObject.SetActive(bundleInfo.LevelNames.Count == 0);
		buttonScript.LevelImageButtonThing.gameObject.SetActive(!bundleInfo.LevelNames.Count == 0);
	}
}

public class AssetBundleInfo
{
	public AssetBundle Bundle { get; set; }
	public List<string> ScenePaths { get; set; }
	public List<string> LevelNames { get; set; }
	public Dictionary<string, Texture2D> LevelImages { get; set; } = new Dictionary<string, Texture2D>();
	public float FileSize { get; set; }
	public string Author { get; set; }
	public bool IsCampaign { get; set; }
}

[Serializable]
public class LevelInfo
{
	public string Author { get; set; }
	public string LevelName { get; set; }
	public bool IsCampaign { get; set; }
	public List<string> Scenes { get; set; }
	public List<string> LevelNames { get; set; }
	public List<string> LevelImages { get; set; }
}
