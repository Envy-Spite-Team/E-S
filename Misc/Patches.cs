using BepInEx;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Logic;
using System.Collections;
using EnvyLevelLoader.Loaders;
using UnityEngine.AddressableAssets;
using System;

namespace EnvyLevelLoader
{

    [HarmonyPatch(typeof(StatsManager))]
	[HarmonyPatch("Start")]
    public static class StatsManager_Start_Patch
    {
        [HarmonyPostfix]
        static void Postfix(StatsManager __instance)
        {
            if (LevelLoader.IsCustomLevel)
            {
                Debugger.Log($"Replacing {__instance.levelNumber} to -1");
                __instance.levelNumber = -1;
            }
        }
    }

    [HarmonyPatch(typeof(FinalRank))]
    [HarmonyPatch("LevelChange")]
    public static class FinalRank_Patch
    {
        static void Prefix(FinalRank __instance)
        {
            if (LevelLoader.IsCustomLevel && string.IsNullOrEmpty(__instance.targetLevelName))
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
            return !LevelLoader.IsCustomLevel;
        }
    }
    
    [HarmonyPatch(typeof(ShopZone))]
    [HarmonyPatch("Start")]
    public static class ShopReplacer_Patch
    {
        static bool Prefix(ShopZone __instance)
        {
            if (!LevelLoader.IsCustomLevel) return true;
            if (__instance.tipOfTheDay == null && __instance.gameObject.name.ToLower() != "shop") return true;
            var music = __instance.gameObject.transform.Find("Jingle Music");
            if (music == null)
            {
                Debugger.Log($"Found OLD shop ({__instance.gameObject.name}) ! Replacing it...");
                var shop = Plugin.ShopTemp;
                if (shop != null)
                {
                    UnityEngine.Object.Destroy(__instance.gameObject);
                    var newShop = UnityEngine.Object.Instantiate(shop, __instance.transform.position, __instance.transform.rotation, __instance.transform.parent);
                    return false;
                }
            }
            return true;
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
            if (!LevelLoader.IsCustomLevel) yield break;

            // just incase someones level is setup weird
            try
            { __instance.cleanTheme.outputAudioMixerGroup = MonoSingleton<AudioMixerController>.Instance.musicGroup; }catch { }
            try
            { __instance.battleTheme.outputAudioMixerGroup = MonoSingleton<AudioMixerController>.Instance.musicGroup; }catch { }
            try
            { __instance.bossTheme.outputAudioMixerGroup = MonoSingleton<AudioMixerController>.Instance.musicGroup; }catch { }
            try
            { __instance.targetTheme.outputAudioMixerGroup = MonoSingleton<AudioMixerController>.Instance.musicGroup; }catch { }

            Scene currentScene = SceneManager.GetActiveScene();
            foreach (AudioSource audio in (AudioSource[])Resources.FindObjectsOfTypeAll(typeof(AudioSource)))
            {
                if(audio.gameObject.scene != currentScene)
                    continue;
                if(audio.gameObject.scene.name != currentScene.name)
                    continue;
                try
                {
                    if (audio.outputAudioMixerGroup.audioMixer.name == "MusicAudio" || audio.outputAudioMixerGroup.audioMixer.name == "MusicAudio_0")
                        audio.outputAudioMixerGroup = MonoSingleton<AudioMixerController>.Instance.musicGroup;
                } catch { }
            }
        }
    }
    
    [HarmonyPatch(typeof(MusicManager))]
    [HarmonyPatch("Update")]
    public static class MusicMan_Patch2
    {
        static bool Prefix(MusicManager __instance)
        {
            if (__instance.targetTheme == null)
            {
                __instance.targetTheme = __instance.cleanTheme;
                if (__instance.targetTheme == null)
                    __instance.targetTheme = __instance.battleTheme;
                if (__instance.targetTheme == null)
                    __instance.targetTheme = __instance.bossTheme;
                if (__instance.targetTheme == null)
                    return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(Material))]
    static class MaterialPatches
    {
        public static void Process(Material material)
        {
            if (material.shader == null)
                return;

            if (!ShaderManager.shaderDictionary.TryGetValue(material.shader.name, out Shader realShader))
                return;

            if (material.shader == realShader)
                return;

            material.shader = realShader;
        }

        [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(Shader) })]
        [HarmonyPostfix]
        public static void CtorPatch1(Material __instance)
        {
            Process(__instance);
        }

        [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(Material) })]
        [HarmonyPostfix]
        public static void CtorPatch2(Material __instance)
        {
            Process(__instance);
        }

        [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(string) })]
        [HarmonyPostfix]
        public static void CtorPatch3(Material __instance)
        {
            Process(__instance);
        }
    }

    // TODO : REVAMP HOW THIS WORKS
    [HarmonyPatch]
    public static class AddressablesScene_Patch
    {
        [HarmonyPatch(typeof(Addressables), nameof(Addressables.LoadSceneAsync),
            new Type[] { typeof(object), typeof(LoadSceneMode), typeof(bool), typeof(int) })]
        [HarmonyPrefix]
        public static bool Prefix_Overload1(object key, LoadSceneMode loadMode, bool activateOnLoad, int priority)
        {
            return PrefixCommon(key, new LoadSceneParameters(loadMode), activateOnLoad, priority);
        }

        [HarmonyPatch(typeof(Addressables), nameof(Addressables.LoadSceneAsync),
            new Type[] { typeof(object), typeof(LoadSceneParameters), typeof(bool), typeof(int) })]
        [HarmonyPrefix]
        public static bool Prefix_Overload2(object key, LoadSceneParameters loadSceneParameters, bool activateOnLoad, int priority)
        {
            return PrefixCommon(key, loadSceneParameters, activateOnLoad, priority);
        }

        public static bool PrefixCommon(object key, LoadSceneParameters loadSceneParameters, bool activateOnLoad, int priority)
        {
            Debugger.Log($"Harmony patch: Loading scene with key: {key}, loadMode: {loadSceneParameters.loadSceneMode}");

            string key_str = key.ToString();
            if (!key_str.StartsWith(EnvyUtility.EnvyScenePrefix))
                return true;

            try
            {
                string query = key_str.Substring(EnvyUtility.EnvyScenePrefix.Length);
                // querys work as [file name]~[scene name]
                // where ~ is LevelLoader.SplitChar
                Debugger.Log($"Got envy level query as {query}");

                string[] file_and_scene = query.Split(LevelLoader.SplitChar);
                string fileName = file_and_scene[0];
                string sceneName = file_and_scene[1];

                if (fileName == "?")
                { LevelLoader.LoadLevel(LevelLoader.CurrentLevel, sceneName); return false; }

                EnvyLevel level = LevelLoader.GetLevelFromFile(Path.Combine(EnvyUtility.ConfigPath, fileName));
                if (level != null)
                    LevelLoader.LoadLevel(level, sceneName);
            }
            catch (Exception e)
            { Debugger.LogError($"Failed to load level {key} because {e.Message}"); }

            return false;
        }
    }
}
