using BepInEx;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EnvyLevelLoader
{
    public static class EnvyUtility
    {
        public static string ConfigPath {
            get { return Path.Combine(Paths.ConfigPath + Path.DirectorySeparatorChar + "EnvyLevels"); }
            private set { }
        }

        /// <summary>
        /// Used to reference the first scene in a EnvyLevel
        /// (due to bundles not being loaded yet it is impossible to know what scenes are in the level's .bundle)
        /// </summary>
        public static string UnknownScene { get { return "__envy_unknown_scene__"; } set { } }

        /// <summary>
        /// Used in the addressables patch to make 100% sure we are only messsing with addressables if its an envy level.
        /// </summary>
        public static string EnvyScenePrefix { get { return "__envylevel__"; } set { } }


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
    }
}
