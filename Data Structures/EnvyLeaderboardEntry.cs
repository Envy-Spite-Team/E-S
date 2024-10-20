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

        public EnvyUser User;

        public EnvyLeaderboardEntry(ulong userID, ulong time, bool prank)
        {
            PRank = prank;
            Time = time;
            User = new EnvyUser(userID);
        }

        public void ApplyToLeaderboard(GameObject leaderboard)
        {
            throw new NotImplementedException();
        }
    }
}