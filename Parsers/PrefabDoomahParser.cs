using EnvyLevelLoader.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEngine;

namespace EnvyLevelLoader.Parsers
{
    /// <summary>
    /// Class to parse pre-scene storing .doomah files.
    /// </summary>
    public static class PrefabDoomahParser
    {
        internal class PrefabEnvyLevel : EnvyLevel
        {
        }
        
        public static EnvyLevel ParseLevelInfo(string fileName)
        {
            Debug.LogWarning("info.txt not found in archive. Applying fix...");
            
            PrefabEnvyLevel levelInfo = new PrefabEnvyLevel
            {
                Author = "Unknown Author",
                Name = Path.GetFileNameWithoutExtension(fileName),
                IsCampagin = false, // Set to false by default
                Version = EnvyLevelVersions.PreSceneDoomah
            }; 
            EnvyLevel.SubLevel level = new EnvyLevel.SubLevel();
            level.Thumbnail = new Texture2D(2, 2);

            level.SceneName = EnvyUtility.UnknownScene;
            levelInfo.Scenes.Add(level);

            return levelInfo;
        }
    }
}