using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace EnvyLevelLoader.Loaders
{
    /// <summary>
    /// Data structure to represent a .doomah or .envy
    /// </summary>
    [Serializable]
    public class EnvyLevel
    {
        /// <summary>
        /// Data structure to represent a scene in a bundle.
        /// </summary>
        [Serializable]
        public struct SubLevel
        {
            [SerializeField]
            public string SceneName;
            [SerializeField]
            public string Name; //thanks doomah

            [NonSerialized]
            public Texture2D Thumbnail;
            [SerializeField]
            public string ThumbnailPath;
        }

        [SerializeField]
        public string Name;
        [SerializeField]
        public string Author;

        [NonSerialized]
        public string FilePath;

        [NonSerialized]
        private byte[] realBundleData = null;

        public byte[] BundleData // don't load bundle data untill it is accessed
        {
            get
            {
                if (realBundleData == null)
                {
                    if (File.Exists(FilePath))
                    {
                        if (Version == EnvyLevelVersions.PreSceneDoomah)
                        {
                            realBundleData = File.ReadAllBytes(FilePath);
                        }
                        else
                        {
                            using (var archive = new ZipArchive(File.OpenRead(FilePath), ZipArchiveMode.Read))
                            {
                                foreach (ZipArchiveEntry e in archive.Entries)
                                {
                                    if(Path.GetExtension(e.FullName) == ".bundle")
                                    {
                                        Stream s = e.Open();
                                        realBundleData = EnvyUtility.ReadFully(s);
                                        s.Close();
                                    }
                                }
                            }
                        }
                    }
                }
                return realBundleData;
            }
            set
            {
                realBundleData = value;
            }
        }
        [NonSerialized]
        public AssetBundle LoadedBundle;

        [SerializeField]
        public List<SubLevel> Scenes = new List<SubLevel>();

        [SerializeField]
        public bool IsCampagin;
        [NonSerialized]
        public Texture2D CampaginThumbnail;
        [SerializeField]
        public string CampaginThumbnailPath;
        
        // versions are to help with the future .envy system but works with doomahs too
        // here is the list of existing/planned supported file formats:
        // [implemented] unknown -> doomah.0.1
        // [implemented] ancient doomah (prefab not scene file) -> doomah.0.5
        // [implemented] missing info.txt doomah -> doomah.0.9
        // [implemented] pre-revamp doomah -> doomah.1.0
        // [not implemented (no good way to check atm)] post-revamp doomah -> doomah.1.1
        //
        // [not implemented] envy -> envy.1.0
        [SerializeField]
        public string Version = EnvyLevelVersions.Unkown;

        // for checking if we should re-load the file
        [NonSerialized]
        public DateTime EditedDate = DateTime.Now;
    }

    public static class EnvyLevelVersions
    {
        public const string Unkown = "doomah.0.1";
        public const string PreSceneDoomah = "doomah.0.5";
        public const string MissingInfo = "doomah.0.9";
        public const string PreRevamp = "doomah.1.0";
        public const string PostRevamp = "doomah.1.1";
        
        public const string EnvyRelease = "envy.1.0";
    }
}
