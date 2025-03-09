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
    /// Class to parse .doomah files.
    /// Doomah files HAVE to only use the old .txt system for compatability purposes! (no info.json from the new envy format basically)
    /// </summary>
    public static class DoomahParser
    {
        public static EnvyLevel ParseLevelInfo(ZipArchive archive, string fileName = "")
        {
            ZipArchiveEntry infoTxtEntry = archive.GetEntry("info.txt");
            EnvyLevel levelInfo = null;
            if (infoTxtEntry == null)
            {
                Debug.LogWarning("info.txt not found in archive. Applying fix...");
                levelInfo = new EnvyLevel
                {
                    Author = "Unknown Author",
                    Name = string.IsNullOrWhiteSpace(fileName) ? "Unknown Name" : Path.GetFileNameWithoutExtension(fileName),
                    IsCampagin = false, // Set to false by default
                    Version = EnvyLevelVersions.MissingInfo
                };
            }
            else
            {
                using (var reader = new StreamReader(infoTxtEntry.Open()))
                {
                    string infoText = reader.ReadToEnd();
                    levelInfo = LoadFromText(infoText);
                }
            }

            EnvyLevel.SubLevel level = new EnvyLevel.SubLevel();
            var imageFile = archive.Entries.FirstOrDefault(file => Path.GetExtension(file.FullName) == ".png");
            if(imageFile != null)
            {
                Stream imageStream = imageFile.Open();
                byte[] imageData = EnvyUtility.ReadFully(imageStream);
                imageStream.Close();
                Texture2D image = new Texture2D(2, 2);
                image.LoadImage(imageData);

                level.Thumbnail = image;
            }else
                level.Thumbnail = new Texture2D(2, 2);

            level.SceneName = EnvyUtility.UnknownScene;

            levelInfo.Scenes.Add(level);

            return levelInfo;
        }

        public static EnvyLevel LoadFromText(string text)
        {
            EnvyLevel info = new EnvyLevel();
            string[] lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0) info.Author = lines[0].Trim();
            if (lines.Length > 1) info.Name = lines[1].Trim();
            info.IsCampagin = false; // Set to false by default
            info.Version = EnvyLevelVersions.PreRevamp;
            return info;
        }
    }
}