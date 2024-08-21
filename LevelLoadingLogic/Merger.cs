using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using UnityEngine;

namespace DoomahLevelLoader
{
    public static class Merger
    {
        private static string PartsPath => Plugin.getConfigPath();

        public static async Task MergeFiles()
        {
            var partFiles = Directory.GetFiles(PartsPath, "*.part*");
            var filePattern = new Regex(@"^(?<basename>.+)\.part(?<part>\d+)$");
            var fileGroups = new Dictionary<string, List<string>>();

            foreach (var file in partFiles)
            {
                var match = filePattern.Match(Path.GetFileName(file));
                if (match.Success)
                {
                    string baseName = match.Groups["basename"].Value;
                    if (!fileGroups.ContainsKey(baseName))
                        fileGroups[baseName] = new List<string>();
                    fileGroups[baseName].Add(file);
                }
            }

            foreach (var group in fileGroups)
            {
                string baseName = group.Key;
                var sortedParts = group.Value.OrderBy(f => int.Parse(filePattern.Match(Path.GetFileName(f)).Groups["part"].Value)).ToList();
                var mergedFilePath = Path.Combine(PartsPath, baseName); //IT ALREADY HAS THE .DOOMAH EXTENSION WRAHHHH

                using (var outputStream = File.Create(mergedFilePath))
                {
                    foreach (var part in sortedParts)
                    {
                        using (var inputStream = File.OpenRead(part))
                            await inputStream.CopyToAsync(outputStream);
                    }
                }

                foreach (var part in sortedParts)
                    File.Delete(part);

                Debug.Log($"Merged and deleted parts for: {baseName}, created file: {baseName}.doomah");
            }
        }
    }
}
