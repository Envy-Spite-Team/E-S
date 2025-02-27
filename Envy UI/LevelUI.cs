using EnvyLevelLoader.Loaders;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace EnvyLevelLoader.UI
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
                SceneHelper.LoadScene(LevelLoader.GetLevelKey(TargetLevel), true);
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

            LevelInfo.gameObject.SetActive(false);
            ScriptsIcon.gameObject.SetActive(false);
            TargetLevel = targetLevel;

            if (RankIcon != null)
            {
                int rankReal = -1;
                string savePath = Path.Combine(GameProgressSaver.SavePath, "Envy",
                    $"lvl{Path.GetFileName(TargetLevel.FilePath)}progress.bepis");
                if (File.Exists(savePath))
                {
                    RankData rd = GameProgressSaver.ReadFile(savePath) as RankData;
                    if (rd?.ranks != null)
                        foreach (var rank in rd.ranks)
                        {
                            if (rank > rankReal)
                            {
                                rankReal = rank;
                            }
                        }

                    if(rankReal != -1)
                        RankIcon.SetRank(rankReal);
                }
            }
            
            Debugger.Log($"{targetLevel.Name}'s key is {LevelLoader.GetLevelKey(targetLevel)}");
        }
    }
}
