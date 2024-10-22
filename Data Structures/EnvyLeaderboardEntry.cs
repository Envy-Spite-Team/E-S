using Steamworks;
using Steamworks.Data;
using System;
using System.Threading.Tasks;
using UnityEngine;
using EnvyLevelLoader;
using UnityEngine.UI;
using TMPro;
using static Mono.Security.X509.X520;
using static Vertx.Debugging.Shape;

namespace EnvyLevelLoader.UI
{
    /// <summary>
    /// Sets the values of a leaderboard entry to its own values.
    /// Used to be a data structure now a ui element, might move from folder.
    /// </summary>
    public class EnvyLeaderboardEntry : MonoBehaviour
    {
        [Header("Params")]
        public string Difficulty = "VIOLENT";
        public ulong Time = 1;
        public bool PRank = false;
        public ulong TargetUserID;

        public EnvyUser User;
        public bool Loaded = false;

        public void Start()
        {
            User = new EnvyUser(TargetUserID);
        }

        void OnEnable()
        {
            Apply();
        }

        public void Apply()
        {
            if (Loaded) { return; }
            Task.Run(async () => {
                Debugger.Log($"{name} waiting for EnvyUser...");
                while (!User.HasLoaded)
                    await Task.Yield();

                RawImage image = transform.Find("RawImage").GetComponent<RawImage>();
                TextMeshProUGUI uname = transform.Find("Username Field").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI diff = transform.Find("Difficulty").GetComponent<TextMeshProUGUI>();
                TextMeshProUGUI time = transform.Find("Time").GetComponent<TextMeshProUGUI>();

                image.texture = User.Avatar;
                uname.text = User.UserName;
                diff.text = Difficulty;

                TimeSpan timeSpan = TimeSpan.FromMilliseconds(Time);
                time.text = $"{(int)timeSpan.TotalMinutes}:{timeSpan.Seconds:D2}.{timeSpan.Milliseconds:D3}";
                Loaded = true;
            });
        }
    }
}