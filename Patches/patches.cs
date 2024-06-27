using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Logic;

namespace DoomahLevelLoader
{
    [HarmonyPatch(typeof(SceneHelper), "RestartScene")]
    public static class SceneHelper_RestartScene_Patch
    {
        [HarmonyPrefix]
        public static bool Prefix()
        {
			if (Plugin.IsCustomLevel)
			{
				UnityEngine.SceneManagement.SceneManager.LoadScene(Loaderscene.LoadedSceneName);
				return false;
			}
			return true;
		}
    }

	[HarmonyPatch(typeof(FinalRank), "LevelChange")]
    public static class FinalRank_LevelChangePatch
    {
        [HarmonyPrefix]
        public static bool Prefix(FinalRank __instance)
        {
			if (!Plugin.IsCustomLevel)
				return true;
			
			if (__instance.targetLevelName == null)
				return true;
			
			Loaderscene.LoadedSceneName = __instance.targetLevelName;
			Loaderscene.Loadscene();
			return false;
		}
    }

	[HarmonyPatch(typeof(LevelNameFinder))]
	[HarmonyPatch("OnEnable")]
	public static class LevelNameFinder_Patch
	{
		static void Postfix(LevelNameFinder __instance)
		{
			if (!Plugin.IsCustomLevel)
				return;
			
			string bundleFolderPath = Loaderscene.GetCurrentBundleFolderPath();

			if (string.IsNullOrEmpty(bundleFolderPath))
			{
				return;
			}

			string infoFilePath = Path.Combine(bundleFolderPath, "info.txt");

			if (!File.Exists(infoFilePath))
			{
				__instance.txt2.text = "Failed to load Level name!";
				return;
			}

			try
			{
				string[] lines = File.ReadAllLines(infoFilePath);

				if (lines.Length >= 2)
				{
					string levelName = lines[1];
					__instance.txt2.text = levelName;
				}
				else
				{
					__instance.txt2.text = "Failed to load Level name!";
				}
			}
			catch
			{
				__instance.txt2.text = "Failed to load Level name!";
			}
		}
	}
}
