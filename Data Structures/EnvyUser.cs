using Newtonsoft.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking.Match;

namespace EnvyLevelLoader.UI
{
    /// <summary>
    /// Abstraction of a steam user. (cannot use steamworks due to this being data grabbed from the envy leaderboard server)
    /// </summary>
    public class EnvyUser
    {
        /// <summary>
        /// Represents the SteamResponse, given by the steam api proxy request to the envy leaderboards server.
        /// </summary>
        private struct SteamResponse
        {
            public ResponseData response { get; set; }
        }

        /// <summary>
        /// Represents the ResponseData, given by the steam api proxy request to the envy leaderboards server.
        /// </summary>
        private struct ResponseData
        {
            public List<Player> players { get; set; }
        }

        /// <summary>
        /// Represents a Player, given by the steam api proxy request to the envy leaderboards server.
        /// </summary>
        private struct Player
        {
            public string steamid { get; set; }
            public string personaname { get; set; }
            public string avatarmedium { get; set; }
        }

        public ulong UserID { get; private set; }
        /// <summary>
        /// The medium sized steam avatar, loaded from https://avatars.steamstatic.com/
        /// </summary>
        public Texture2D Avatar { get; private set; }
        public string UserName { get; private set; }

        /// <summary>
        /// If true the Avatar and UserName have been loaded. THIS DOES NOT INDICATE A FAILED LOAD, PLEASE DO A NULL CHECK!
        /// </summary>
        public bool HasLoaded { get; private set; }

        /// <summary>
        /// Loads the userdata at the userid via steam api through envy leaderboards proxy, please check HasLoaded!
        /// </summary>
        /// <param name="id">The userid.</param>
        public EnvyUser(ulong id)
        {
            UserID = id;
            HasLoaded = false;

            Task.Run(async () => {
                await Task.Yield();
                Debugger.Log("LoadUserJson");
                LoadUserJson(await EnvyUtility.GetUserJson(UserID));
            });
            return;
        }

        public void LoadUserJson(string json)
        {
            if (json == "{}" || json == "[]" || string.IsNullOrEmpty(json))
            { Debugger.LogWarn("Invalid json passed to LoadUserJson!"); return; }

            Debugger.Log("Got JSON: " + json);
            try
            {
                var response = JsonConvert.DeserializeObject<Dictionary<string, ResponseData>>(json);
                List<Player> plrs = response["response"].players;
                Player target = plrs[0];

                UserName = target.personaname;
                Task.Run(async () =>
                {
                    try
                    {
                        Avatar = await EnvyUtility.GetTextureFromUrl(target.avatarmedium);
                        HasLoaded = true;
                    }
                    catch (System.Exception e)
                    {
                        Debugger.LogError("Avatar loading code probably fucked up, error: " + e.Message);
                        HasLoaded = true;
                    }
                });
            }
            catch (System.Exception e)
            {
                Debugger.LogError("That response got fucked up damn, error: " + e.Message);
                HasLoaded = true;
            }
        }
    }
}