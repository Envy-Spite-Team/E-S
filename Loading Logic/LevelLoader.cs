using EnvyLevelLoader.Parsers;
using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq.Expressions;

namespace EnvyLevelLoader.Loaders
{
    /// <summary>
    /// Class to load EnvyLevel classes.
    /// Holds data about the current level.
    /// </summary>
    public static class LevelLoader
    {
        public static bool IsCustomLevel     { get; private set; }
        public static bool IsCampaginLevel   { get; private set; }
        public static EnvyLevel CurrentLevel { get; private set; }

        public static string GetLevelKey(EnvyLevel level, string sceneOverride = "")
        {
            if (string.IsNullOrEmpty(sceneOverride))
                sceneOverride = EnvyUtility.UnknownScene;

            return $"{EnvyUtility.EnvyScenePrefix}{Path.GetFileName(level.FilePath)}>{sceneOverride}";
        }

        /// <summary>
        /// Loads a EnvyLevel. Used internally, see GetLevelKey and Addressables.LoadSceneAsync for loading levels manually.
        /// </summary>
        /// <param name="levelTarget">The level to load.</param>
        /// <returns>If the level is a valid level.</returns>
        public static bool LoadLevel(EnvyLevel levelTarget, string targetScene = "")
        {
            if (CurrentLevel != null)
                CurrentLevel.LoadedBundle.Unload(true);

            if (string.IsNullOrEmpty(targetScene))
                targetScene = EnvyUtility.UnknownScene;

            Debugger.Log($"Loading envy level {levelTarget.Name} with scene {targetScene}");

            levelTarget.LoadedBundle = AssetBundle.LoadFromMemory(levelTarget.BundleData);
            Debugger.Log(levelTarget.LoadedBundle);
            if(targetScene == EnvyUtility.UnknownScene)
                targetScene = levelTarget.LoadedBundle.GetAllScenePaths().FirstOrDefault();

            IsCustomLevel = true;

            SceneManager.LoadSceneAsync(targetScene).completed += op =>
            {
                SceneHelper.DismissBlockers();
                Camera mainCamera = Camera.main;
                IsCustomLevel = true;
                mainCamera.clearFlags = CameraClearFlags.Skybox;
                Plugin.Instance.StartCoroutine(ShaderManager.ApplyShadersAsyncContinuously());
            };

            return true;
        }

        /// <summary>
        /// Gets an EnvyLevel from a file, can be .doomah or .envy.
        /// </summary>
        /// <param name="path">The file to get the EnvyLevel from.</param>
        /// <returns>The EnvyLevel. (can be null)</returns>
        public static EnvyLevel GetLevelFromFile(string path)
        {
            bool isDoomah = Path.GetExtension(path).ToLower() == ".doomah";

            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                ZipArchive archive = new ZipArchive(fs, ZipArchiveMode.Read);

                EnvyLevel level = null;

                if (isDoomah)
                    level = DoomahParser.ParseLevelInfo(archive);
                else
                    EnvyParser.ParseLevelInfo(archive);

                foreach (ZipArchiveEntry e in archive.Entries)
                {
                    if(Path.GetExtension(e.FullName) == ".bundle")
                    {
                        Stream s = e.Open();
                        level.BundleData = EnvyUtility.ReadFully(s);
                        s.Close();
                    }
                }

                level.FilePath = path;

                return level;
            }
        }
    }
}
