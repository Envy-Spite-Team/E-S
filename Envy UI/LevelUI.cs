using EnvyLevelLoader.Loaders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace EnvyLevelLoader.Envy_UI
{
    [RequireComponent(typeof(Button))]
    public class LevelUI : MonoBehaviour
    {
        [HideInInspector]
        public EnvyLevel TargetLevel { get; private set; }

        [HideInInspector]
        public bool IsCampagin;

        public TextMeshProUGUI Name;
        public TextMeshProUGUI Author;

        public RawImage Thumbnail;
        public Image ScriptsIcon;

        public RankIcon RankIcon;

        public Button LevelInfo;

        void Start()
        {
            GetComponent<Button>().onClick.AddListener(() => {
                Addressables.LoadSceneAsync(LevelLoader.GetLevelKey(TargetLevel));
            });
        }

        public void Load(EnvyLevel targetLevel)
        {
            IsCampagin = targetLevel.IsCampagin;

            Name.text = targetLevel.Name;
            Author.text = targetLevel.Author;

            if (IsCampagin)
            {}
            else
                Thumbnail.texture = targetLevel.Scenes.FirstOrDefault().Thumbnail;

            ScriptsIcon.gameObject.SetActive(false);
            TargetLevel = targetLevel;

            Debugger.Log($"{targetLevel.Name}'s key is {LevelLoader.GetLevelKey(targetLevel)}");
        }
    }
}
