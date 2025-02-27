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

        private bool _hasLoaded = false;
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
                    // turns out the list of ranks where a list of each rank for its corresponding difficulty lmao
                    rankReal = rd?.ranks[MonoSingleton<PrefsManager>.Instance.GetInt("difficulty")] ?? -1;

                    if(rankReal != -1)
                        RankIcon.SetRank(rankReal);
                    else
                    {
                        RankIcon.SetRank(-1);
                        RankIcon.mainRankLetter.text = "?";
                    }
                }
            }
            
            _hasLoaded = true;
            Debugger.Log($"{targetLevel.Name}'s key is {LevelLoader.GetLevelKey(targetLevel)}");
        }

        public void OnEnable()
        {
            if (_hasLoaded)
            {
                if (RankIcon != null)
                {
                    int rankReal = -1;
                    string savePath = Path.Combine(GameProgressSaver.SavePath, "Envy",
                        $"lvl{Path.GetFileName(TargetLevel.FilePath)}progress.bepis");
                    if (File.Exists(savePath))
                    {
                        RankData rd = GameProgressSaver.ReadFile(savePath) as RankData;
                        // turns out the list of ranks where a list of each rank for its corresponding difficulty lmao
                        rankReal = rd?.ranks[MonoSingleton<PrefsManager>.Instance.GetInt("difficulty")] ?? -1;

                        if(rankReal != -1)
                            RankIcon.SetRank(rankReal);
                        else
                        {
                            RankIcon.SetRank(-1);
                            RankIcon.mainRankLetter.text = "?";
                        }
                    }
                }
            }
        }
    }
}
