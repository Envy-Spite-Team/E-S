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
				SceneManager.LoadScene(Loaderscene.currentLevelpath);
				return false;
			}
			return true;
		}
    }

    [HarmonyPatch(typeof(StatsManager))]
	[HarmonyPatch("Start")]
    public static class StatsManager_Start_Patch
    {
        [HarmonyPostfix]
        static void Postfix(StatsManager __instance)
        {
            if (Plugin.IsCustomLevel)
            {
				__instance.levelNumber = -1;
				Debug.Log(__instance.levelNumber);
            }
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
			
			if (string.IsNullOrEmpty(Loaderscene.currentLevelName))
				return;

			__instance.txt2.text = Loaderscene.currentLevelName;
		}
	}
}
