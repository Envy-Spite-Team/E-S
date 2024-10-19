using EnvyLevelLoader.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnvyLevelLoader.Parsers
{
    /// <summary>
    /// Class to parse .envy files.
    /// Envy files HAVE to only use the new .json system!
    /// </summary>
    public class EnvyParser
    {
        public static EnvyLevel ParseLevelInfo(ZipArchive archive)
        {
            ZipArchiveEntry infoJsonEntry = archive.GetEntry("info.json");
            if (infoJsonEntry == null)
            {
                Debugger.LogError("info.json not found in archive.");
                return null;
            }

            using (var reader = new StreamReader(infoJsonEntry.Open()))
            {
                string jsonText = reader.ReadToEnd();
                try
                {
                    // Parse JSON into LevelInfo object
                    EnvyLevel level = JsonUtility.FromJson<EnvyLevel>(jsonText);
                    return level;
                }
                catch (Exception e)
                {
                    Debugger.LogError("Failed to parse info.json: " + e.ToString());
                    return null;
                }
            }
        }
    }
}
