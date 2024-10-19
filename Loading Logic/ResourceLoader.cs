using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace EnvyLevelLoader.Loaders
{
    /// <summary>
    /// Class to load resources from the dll, currently only loads bundles.
    /// </summary>
    public static class ResourceLoader
    {
        /// <summary>
        /// Loads bundles from the dll from the bundle directory.
        /// </summary>
        /// <param name="name">The bundle name.</param>
        /// <returns>The bundle from the dll. This can return null!</returns>
        public static AssetBundle GetBundle(string name)
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resourceName = $"EnvyLevelLoader.Bundles.{name}.bundle";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Debugger.LogError($"Resource '{name}.bundle' not found in embedded resources.");
                        return null;
                    }

                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    return AssetBundle.LoadFromMemory(buffer);
                }
            }
            catch (Exception ex)
            {
                Debugger.LogError($"Error loading {name}: " + ex.Message);
                return null;
            }
        }
    }
}
