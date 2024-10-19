using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
        public byte[] BundleData;
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
    }
}
