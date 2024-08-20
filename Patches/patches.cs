using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Logic;
using System.Collections;

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
			SceneManager.LoadSceneAsync(Loaderscene.currentLevelpath).completed += operation =>
			{
			    SceneHelper.DismissBlockers();
			    Plugin.Fixorsmth();
			};
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
            // why ignore whitespace brah
			if (Plugin.IsCustomLevel) __instance.txt2.text = Loaderscene.currentLevelName;
        }
	}

    [HarmonyPatch(typeof(FinalRank))]
    [HarmonyPatch("LevelChange")]
    public static class FinalRank_Patch
    {
        static void Prefix(FinalRank __instance)
        {
            if (Plugin.IsCustomLevel && string.IsNullOrEmpty(__instance.targetLevelName))
                __instance.targetLevelName = "Main Menu";
        }
    }

    [HarmonyPatch(typeof(AdvancedOptions))]
    [HarmonyPatch("ResetCyberGrind")]
    public static class AdvOpt_Patch
    {
        static bool Prefix(AdvancedOptions __instance)
        {
            //oh hell naw
            return !Plugin.IsCustomLevel;
        }
    }

    [HarmonyPatch(typeof(MusicManager))]
    [HarmonyPatch("OnEnable")]
    public static class MusicMan_Patch
    {
        static void Postfix(MusicManager __instance)
        {
            __instance.StartCoroutine(waitForCustom(__instance));
        }
        static IEnumerator waitForCustom(MusicManager __instance)
        {
            yield return new WaitForSeconds(0.25f);
            if (!Plugin.IsCustomLevel) yield break;

            try // just incase someones level is setup weird
            {
                __instance.battleTheme.outputAudioMixerGroup = MonoSingleton<AudioMixerController>.instance.musicGroup;
                __instance.bossTheme.outputAudioMixerGroup = MonoSingleton<AudioMixerController>.instance.musicGroup;
                __instance.cleanTheme.outputAudioMixerGroup = MonoSingleton<AudioMixerController>.instance.musicGroup;
                __instance.targetTheme.outputAudioMixerGroup = MonoSingleton<AudioMixerController>.instance.musicGroup;
            }
            catch { }

            foreach (AudioSource audio in GameObject.FindObjectsOfTypeAll(typeof(AudioSource)))
            {
                try
                {
                    if (audio.outputAudioMixerGroup.audioMixer.name == "MusicAudio" || audio.outputAudioMixerGroup.audioMixer.name == "MusicAudio_0")
                        audio.outputAudioMixerGroup = MonoSingleton<AudioMixerController>.instance.musicGroup;
                } catch { }
            }
        }
    }
}
