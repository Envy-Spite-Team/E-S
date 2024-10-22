using BepInEx;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Networking.Match;
using UnityEngine.SceneManagement;

namespace EnvyLevelLoader
{
    public static class EnvyUtility
    {
        private class MainThreadDispatcher : MonoBehaviour
        {
            static MainThreadDispatcher Instance;
            private static readonly ConcurrentQueue<Action> _actions = new ConcurrentQueue<Action>();

            public static void RunOnMainThread(Action action)
            {
                _actions.Enqueue(action);
            }

            void Awake()
            {
                if (Instance == null)
                    Instance = this;
                if (Instance != this)
                    Destroy(this);
                else
                    DontDestroyOnLoad(this.gameObject);
            }

            void Update()
            {
                while (_actions.TryDequeue(out var action))
                {
                    action?.Invoke();
                }
            }
        }

        static SynchronizationContext mainThreadContext;
        public static void CaptureMainThread()
        {
            if (mainThreadContext == null)
            { mainThreadContext = SynchronizationContext.Current; new GameObject("Envy MainThreadDispatcher").AddComponent<MainThreadDispatcher>(); }
        }

        public static bool IsMainThread
        {
            get { return SynchronizationContext.Current == mainThreadContext; }
            set { }
        }
        public static void RunOnMainThread(Action action)
        {
            MainThreadDispatcher.RunOnMainThread(action);
        }

        public static string ConfigPath {
            get { return Path.Combine(Paths.ConfigPath + Path.DirectorySeparatorChar + "EnvyLevels"); }
            private set { }
        }

        public static string PluginPath
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
            private set { }
        }

        /// <summary>
        /// Used to reference the first scene in a EnvyLevel
        /// (due to bundles not being loaded yet it is impossible to know what scenes are in the level's .bundle)
        /// </summary>
        public static string UnknownScene { get { return "__envy_unknown_scene__"; } private set { } }

        /// <summary>
        /// Used in the addressables patch to make 100% sure we are only messsing with addressables if its an envy level.
        /// </summary>
        public static string EnvyScenePrefix { get { return "__envylevel__"; } private set { } }

        /// <summary>
        /// Gets the envy server url, can be overriden to local file for debugging purposes.
        /// </summary>
        public static string EnvyLeaderboardsServer {
            get {
                if (File.Exists(Path.Combine(PluginPath, "override_server.txt")))
                    return File.ReadAllLines(Path.Combine(PluginPath, "override_server.txt"))[0];

                return "https://poodle-informed-oddly.ngrok-free.app/"; // free services go brrrr
            }
            set { }
        }


        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream reader = new MemoryStream())
            {
                input.CopyTo(reader);
                return reader.ToArray();
            }
        }

        /// <summary>
        /// Utility method to find a child with a path of a GameObject even if it's inactive
        /// </summary>
        /// <param name="parent">The parent transform with the child.</param>
        /// <param name="name">The childs name.</param>
        /// <returns>The child.</returns>
        public static GameObject FindObjectEvenIfDisabled(string rootName, string path = "")
        {
            GameObject[] rootObjects = SceneManager.GetActiveScene().GetRootGameObjects();

            GameObject current = rootObjects.Where(go => go.name == rootName).FirstOrDefault();

            if (current == null)
                return null;
            if (string.IsNullOrEmpty(path))
                return current;

            string[] pathParts = path.Split('/');
            // Traverse the hierarchy using the provided path
            foreach (string part in pathParts)
            {
                if (current == null)
                    return null;

                // Find the child even if it's inactive
                current = FindChildEvenIfDisabled(current.transform, part);
            }

            return current;
        }

        /// <summary>
        /// Utility method to find a child of a GameObject even if it's inactive
        /// </summary>
        /// <param name="parent">The parent transform with the child.</param>
        /// <param name="name">The childs name.</param>
        /// <returns>The child.</returns>
        private static GameObject FindChildEvenIfDisabled(Transform parent, string  name)
        {
            foreach (Transform child in parent)
                if (child.name == name)
                    return child.gameObject;

            return null;
        }

        /// <summary>
        /// Sends a request to the steam api to get userdata, used by EnvyUser internally.
        /// </summary>
        /// <param name="user">The user's id to fetch the data from.</param>
        /// <returns>The task that polls the server for the userdata json.</returns>
        public static async Task<string> GetUserJson(ulong user)
        {
            Debugger.Log("GetUserJson");
            string url = EnvyUtility.EnvyLeaderboardsServer + "getSteamProfiles/steamids=" + user.ToString();

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                Debugger.Log("SendWebRequest");
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield(); // Wait for the request to complete asynchronously

                Debugger.Log("return request.downloadHandler.text");
                return request.downloadHandler.text; // Return the JSON as a string
            }
        }

        public static async Task<string> GetLeaderboardsJson(string levelName)
        {
            Debugger.Log("GetLeaderboardsJson");
            string url = EnvyUtility.EnvyLeaderboardsServer + "getLeaderboard/level=Greybox"; // TODO : unhard code this (its hard coded for testing)

            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                Debugger.Log("SendWebRequest");
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield(); // Wait for the request to complete asynchronously

                Debugger.Log("return request.downloadHandler.text");
                return request.downloadHandler.text; // Return the JSON as a string
            }
        }

        public static async Task<Texture2D> GetTextureFromUrl(string url) // do i need a summary for this :sob:
        {
            using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(url))
            {
                // Start the request and wait for it to complete
                var operation = request.SendWebRequest();

                while (!operation.isDone)
                    await Task.Yield(); // Yield until the request is completed

                // Get the downloaded texture
                return DownloadHandlerTexture.GetContent(request);
            }
        }
    }
}
