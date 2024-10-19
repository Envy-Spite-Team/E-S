using Steamworks;
using Steamworks.Data;
using System;
using System.Threading.Tasks;
using UnityEngine;
using EnvyLevelLoader;

namespace EnvyLevelLoader.UI
{
    public class EnvyLeaderboardEntry
    {
        public string Difficulty = "VIOLENT";
        public ulong Time = 1;
        public bool PRank = false;

        public string Username = "USERNAME";
        public ulong UserID;

        public Texture2D UserAvatar;
        public bool HasLoadedAvatar;

        public EnvyLeaderboardEntry(ulong userID, ulong time, bool prank)
        {
            HasLoadedAvatar = false;
            UserID = userID;
            PRank = prank;
            Time = time;
            Task.Run(async () => {
                try
                {
                    Debugger.Log("Polling for steam avatar");
                    Image? SteamWorksAvatar = null;//await SteamFriends.GetSmallAvatarAsync(UserID);
                    if (SteamWorksAvatar != null)
                    {
                        UserAvatar = SteamWorksAvatar.ConvertToTexture2D();
                        HasLoadedAvatar = true;
                    }
                    else
                        throw new Exception("Missing steamworks avatar?");
                }
                catch (Exception e)
                { Debugger.LogError(e.Message); }
            });
        }

        public void ApplyToLeaderboard(GameObject leaderboard)
        {
            throw new NotImplementedException();
        }
    }
}