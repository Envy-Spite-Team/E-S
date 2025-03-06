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
using EnvyLevelLoader.UI;
using TMPro;
using Object = UnityEngine.Object;

namespace EnvyLevelLoader
{

    [HarmonyPatch(typeof(GameProgressSaver))]
    [HarmonyPatch("LevelProgressPath")]
    public static class LevelProgress_Patch
    {
        public static bool Prefix(ref string __result, int lvl)
        {
            Debugger.Log($"Getting level path for {lvl} and is playing custom is {LevelLoader.IsCustomLevel} and level path as {LevelLoader.CurrentLevel}");
            if (LevelLoader.IsCustomLevel && lvl == -1)
            {
                if (!Directory.Exists(EnvyUtility.SaveFolderPath))
                    Directory.CreateDirectory(EnvyUtility.SaveFolderPath);
                
                __result = Path.Combine(EnvyUtility.SaveFolderPath, $"lvl{Path.GetFileName(LevelLoader.CurrentLevel.FilePath)}progress.bepis");
                Debugger.Log($"returning {__result}");
                return false;
            }

            return true;
        }
    }
    
    //AmbiguousMatchException: Ambiguous match found.
    /*[HarmonyPatch(typeof(GameProgressSaver))]
    [HarmonyPatch("GetRank")]
    public static class GetRank_Patch2
    {
        public static bool Prefix(ref RankData __result, bool returnNull, int lvl = -1)
        {
            if(LevelLoader.IsCustomLevel && lvl == -1)
            {
                __result = GetRank_Patch.GetRank(out string path, lvl, returnNull);
                return false;
            }

            return true;
        }
    }*/
    
    [HarmonyPatch]
    public static class GetRank_Patch
    {
        [HarmonyTargetMethod]
        public static MethodBase TargetMethod()
        {
            return AccessTools.Method(typeof(GameProgressSaver), "GetRankData", new[] { typeof(string).MakeByRefType(), typeof(int), typeof(bool) });
        }
        [HarmonyPrefix]
        public static bool Prefix(ref RankData __result, out string path, int lvl = -1, bool returnNull = false)
        {
            if (!(LevelLoader.IsCustomLevel && lvl == -1))
            {
                GameProgressSaver.PrepareFs();
                path = GameProgressSaver.LevelProgressPath(lvl);
                return true;
            }
            path = "";
            __result = GetRank(out path, out bool didChange, lvl, returnNull);
            if (didChange)
            {
                return false;
            }
            return true;
        }

        public static RankData GetRank(out string path, out bool didChange, int lvl = -1, bool returnNull = false)
        {
            Debugger.Log($"Getting level rank for {lvl} and is playing custom is {LevelLoader.IsCustomLevel} and level path as {LevelLoader.CurrentLevel}");
            didChange = false;
            path = "";
            if (LevelLoader.IsCustomLevel && lvl == -1)
            {
                GameProgressSaver.PrepareFs();
                path = GameProgressSaver.LevelProgressPath(lvl);
                RankData result;
                if ((result = GameProgressSaver.ReadFile(path) as RankData) == null)
                {
                    result = (returnNull ? null : new RankData(MonoSingleton<StatsManager>.Instance));
                }
                Debugger.Log(result);
                didChange = true;
                return result;
            }

            if (!returnNull)
                return new RankData(MonoSingleton<StatsManager>.Instance);
            
            return null;
        }
    }
    
    [HarmonyPatch(typeof(StatsManager))]
	[HarmonyPatch("Awake")]
    public static class StatsManager_Awake_Patch
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
    
    /*[HarmonyPatch(typeof(SceneHelper))]
    [HarmonyPatch("GetLevelIndexAfterIntermission")]
    public static class SceneHelper_GetLevelIndexAfterIntermission_Patch
    {
        [HarmonyPostfix]
        static bool Postfix(StatsManager __instance, ref int? __result, string intermissionScene)
        {
            if (LevelLoader.IsCustomLevel)
            {
                Debugger.Log($"Replacing {__instance.levelNumber} to -1");
                __instance.levelNumber = -1;
                __result = new int?(-1);
            }
        }
    }*/

    [HarmonyPatch(typeof(ItemPlaceZone))]
    [HarmonyPatch("Awake")]
    public static class ItemPlaceZone_Patch
    {
        [HarmonyPrefix]
        static void Prefix(ItemPlaceZone __instance)
        {
            if(!LevelLoader.IsCustomLevel) return;
            
            if (__instance.altarElements == null)
            {
                __instance.altarElements = [];
            }
        }
    }
    
    [HarmonyPatch(typeof(StatueFake))]
    [HarmonyPatch("Done")]
    public static class StatueFake_Patch
    {
        [HarmonyPrefix]
        static void Prefix(StatueFake __instance)
        {
            if(!LevelLoader.IsCustomLevel) return;
            
            __instance.transform.parent?.Find("StatueEnemy")?.gameObject.SetActive(true);
            __instance.transform.parent?.Find("StatueBoss")?.gameObject.SetActive(true);
            __instance.transform.parent?.GetComponentInChildren<StatueBoss>()?.gameObject.SetActive(true);
            __instance.gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(FinalRank))]
    [HarmonyPatch("LevelChange")]
    public static class FinalRank_Patch
    {
        static void Prefix(FinalRank __instance)
        {
            if (LevelLoader.IsCustomLevel && string.IsNullOrWhiteSpace(__instance.targetLevelName))
                __instance.targetLevelName = "Main Menu";
        }
    }
    
    [HarmonyPatch(typeof(FinalRank))]
    [HarmonyPatch("Start")]
    public static class FinalRank_Patch_MN
    {
        static void Prefix(FinalRank __instance)
        {
            if(!LevelLoader.IsCustomLevel) return;

            var lvlNameFinder = UnityEngine.Object.FindObjectOfType<LevelNameFinder>();
            Debugger.Log($"{lvlNameFinder?.txt2} and {StockMapInfo.Instance?.levelName ?? "UNABLE TO FIND MISSION NAME"}");
            lvlNameFinder!.txt2!.text = StockMapInfo.Instance?.levelName ?? "UNABLE TO FIND MISSION NAME";
            lvlNameFinder!.enabled = false;
            Object.DestroyImmediate(lvlNameFinder);
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
                string oldTip = "The tip of the day could not be loaded.";
                try
                { // try to find totd
                    var shopCanvas = __instance.transform?.Find("Canvas");
                    var shopBorder = shopCanvas?.Find("Border");
                    var tipOfTheDay1 = shopBorder?.Find("TipBox")?.Find("Panel")?.Find("Text")?.GetComponentInChildren<TextMeshProUGUI>();
                    var tipOfTheDay2 = shopCanvas?.Find("TipBox")?.Find("Panel")?.Find("Text")?.GetComponentInChildren<TextMeshProUGUI>();
                    var tipOfTheDay3 = shopBorder?.Find("TipBox")?.Find("Panel")?.Find("TipText")?.GetComponentInChildren<TextMeshProUGUI>();
                    var tipOfTheDay4 = shopCanvas?.Find("TipBox")?.Find("Panel")?.Find("TipText")?.GetComponentInChildren<TextMeshProUGUI>();
                    if(__instance.tipOfTheDay != null)
                        oldTip = __instance.tipOfTheDay.text;
                    
                    if(tipOfTheDay1 != null)
                        oldTip = tipOfTheDay1.text;
                    if(tipOfTheDay2 != null)
                        oldTip = tipOfTheDay2.text;
                    if(tipOfTheDay3 != null)
                        oldTip = tipOfTheDay3.text;
                    if(tipOfTheDay4 != null)
                        oldTip = tipOfTheDay4.text;
                }catch(Exception){}
                
                StockMapInfo mapInfo = StockMapInfo.Instance;
                if (mapInfo?.tipOfTheDay == null)
                {
                    mapInfo!.tipOfTheDay = ScriptableObject.CreateInstance<ScriptableObjects.TipOfTheDay>();
                    mapInfo!.tipOfTheDay.tip = oldTip;
                }
                
                var shop = ResourceLoader.LoadGameobjectAtAddress("Assets/Prefabs/Levels/Shop.prefab");
                if (shop != null)
                {
                    UnityEngine.Object.Destroy(__instance.gameObject);
                    var newShop = UnityEngine.Object.Instantiate(shop, __instance.transform.position, __instance.transform.rotation, __instance.transform.parent);
                    ShopZone zone = newShop.GetComponentInChildren<ShopZone>();
                    zone.tipOfTheDay.text = oldTip;
                    return false;
                }
            }
            return true;
        }
    }
    
    
    [HarmonyPatch(typeof(PlayerActivator))]
    [HarmonyPatch("Activate")]
    public static class PlayerActivator_Patch
    {
        static bool Prefix(PlayerActivator __instance)
        {
            if(!LevelLoader.IsCustomLevel) return true;
            if (__instance.gameObject.transform.parent!.name.ToLower() == "firstroom player only")
            {
                __instance.activated = true;
                return false;
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

    // TODO: REVAMP HOW THIS WORKS (maybe?)
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

                EnvyLevel level = null;
                foreach (var loadedLevel in LevelsList.LoadedLevels) // check if the level is already loaded somewhat
                {
                    if(Path.GetFileName(loadedLevel.FilePath) == Path.GetFileName(fileName) && File.GetLastWriteTime(fileName) == loadedLevel.EditedDate)
                        level = loadedLevel;
                }
                if (level == null)
                    level = LevelLoader.GetLevelFromFile(Path.Combine(EnvyUtility.ConfigPath, fileName));
                
                if (level != null)
                    LevelLoader.LoadLevel(level, sceneName);
            }
            catch (Exception e)
            { Debugger.LogError($"Failed to load level {key} because {e.Message}"); }

            return false;
        }
    }
}
