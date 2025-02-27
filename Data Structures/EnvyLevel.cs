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

        // for checking if we should re-load the file
        [NonSerialized]
        public DateTime EditedDate = DateTime.Now;
    }
}
