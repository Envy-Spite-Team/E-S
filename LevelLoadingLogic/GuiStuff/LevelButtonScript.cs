using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoomahLevelLoader
{
    public class LevelButtonScript : MonoBehaviour
    {
        public Image LevelImageButtonThing;
        public Button LevelButtonReal;
        public TextMeshProUGUI NoLevel;
        //public TextMeshProUGUI FileSize;
        public TextMeshProUGUI Author;
        public TextMeshProUGUI LevelName;

        [HideInInspector]
        public AssetBundle BundleName;

        [HideInInspector]
        public byte[] BundleDataToLoad;

        [HideInInspector]
        public string SceneToLoad;

        [HideInInspector]
        public bool OpenCamp;

        private static LevelButtonScript instance;
        public static LevelButtonScript Instance
        {
            get
            {
                instance ??= FindObjectOfType<LevelButtonScript>();
                if (instance == null) Debug.LogError("LevelButtonScript instance not found in the scene.");
                return instance;
            }
        }
    }
}