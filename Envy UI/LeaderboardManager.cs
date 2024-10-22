using HarmonyLib;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Linq;
using TMPro;
using Newtonsoft.Json;
using System.Threading.Tasks;
using EnvyLevelLoader.UI;
using UnityEngine.ProBuilder.MeshOperations;
using System.Collections.Generic;

namespace EnvyLevelLoader
{

    [HarmonyPatch(typeof(FinalRank))]
    [HarmonyPatch("Appear")]
    public static class FinalRank_Leaderboard_Patch
    {
        static void Prefix(FinalRank __instance)
        {
            LevelEndLeaderboard lel = GameObject.FindObjectsOfTypeAll(typeof(LevelEndLeaderboard)).FirstOrDefault() as LevelEndLeaderboard;
            if (lel != null && lel.gameObject.scene == SceneManager.GetActiveScene())
            {
                lel.gameObject.AddComponent<EnvyLeaderboard>().auto = true;
                LevelEndLeaderboard.Destroy(lel);
            }
            else if (lel != null && lel.gameObject.scene != SceneManager.GetActiveScene())
            { Debugger.LogError("LEVEL END LEADERBOARD FOUND IN RESOURCES OH FUCK THIS IS BAD"); }
        }
    }

    public class EnvyLeaderboard : MonoBehaviour
    {
        private struct LeaderboardResponse
        {
            public LeaderboardSubmission[] submissive;
        }
        private struct LeaderboardSubmission
        {
            public int run_id;
            public ulong uid;
            public string difficulty;
            public bool is_prank;
            public ulong time;
        }

        public TextMeshProUGUI connectingText;
        public TextMeshProUGUI settingsReminder;
        public TextMeshProUGUI inputReminder;
        public TextMeshProUGUI rankType;

        public GameObject EntryTemplate;

        public bool auto = false;

        public void Start()
        {
            connectingText = transform.Find("Connecting").GetComponent<TextMeshProUGUI>();
            settingsReminder = transform.Find("SettingsReminder").GetComponent<TextMeshProUGUI>();
            inputReminder = transform.Find("Mode Switch Input").GetComponent<TextMeshProUGUI>();
            rankType = transform.Find("Text").GetComponent<TextMeshProUGUI>();

            inputReminder.text = "[Q]";
            rankType.text = "ANYTHING IG";
            settingsReminder.text = settingsReminder.text + "\n" + "<color=green>SERVERS COOL?</color>";
            connectingText.text = "CONNECTING TO ENVY SERVERS...";
            connectingText.gameObject.SetActive(true);

            EntryTemplate = transform.Find("Container/Entry Template").gameObject;

            if (auto)
                LoadLeaderboardEntries();
        }

        List<EnvyLeaderboardEntry> envyLeaderboardEntries = new List<EnvyLeaderboardEntry>();

        /// <summary>
        /// Loads all of the Leaderboard entries for this level ONLY if its an envydl level.
        /// </summary>
        public void LoadLeaderboardEntries()
        {
            envyLeaderboardEntries = new List<EnvyLeaderboardEntry>();
            // TODO : LINK TO ENVYDL
            Task.Run(() => {
                Task<string> json = EnvyUtility.GetLeaderboardsJson("cool");
                json.Wait();

                LeaderboardSubmission[] submissions = JsonConvert.DeserializeObject<LeaderboardSubmission[]>(json.Result);
                foreach (var submission in submissions) {
                    EnvyUtility.RunOnMainThread(() =>
                    {
                        Debugger.Log($"Got submission from {submission.uid}");

                        switch (submission.difficulty)
                        {
                            case "HARMLESS":
                            case "LENIENT":
                            case "STANDARD":
                            case "VIOLENT":
                            case "BRUTAL":
                                break;
                            default:
                                Debugger.LogError("Invalid submission! Difficulty was " + submission.difficulty);
                                return;
                        }

                        GameObject entry = GameObject.Instantiate(EntryTemplate);
                        EnvyLeaderboardEntry ele = entry.AddComponent<EnvyLeaderboardEntry>();
                        envyLeaderboardEntries.Add(ele);
                        ele.Time = submission.time;
                        ele.Difficulty = submission.difficulty;
                        ele.PRank = submission.is_prank;
                        ele.TargetUserID = submission.uid;

                        ele.transform.SetSiblingIndex((int)ele.Time);
                        entry.transform.SetParent(EntryTemplate.transform.parent, false);
                        entry.SetActive(true);
                    });
                }
                EnvyUtility.RunOnMainThread(() =>
                {
                    connectingText.gameObject.SetActive(false);
                    EntryTemplate.transform.parent.gameObject.SetActive(true);
                });
            });
        }

        bool prank_mode = false;
        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Q)) { prank_mode = !prank_mode; }

            foreach (var entry in envyLeaderboardEntries)
            {
                if (entry.PRank != prank_mode)
                    entry.gameObject.SetActive(false);
                else
                    entry.gameObject.SetActive(true);
            }

            if(prank_mode)
                rankType.text = "P RANK";
            else
                rankType.text = "ANY RANK";
        }
    }
}