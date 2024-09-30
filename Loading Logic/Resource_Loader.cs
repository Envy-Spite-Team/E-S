using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace DoomahLevelLoader
{
    public static class Loader
    {
        public static AssetBundle LoadTerminal()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resourceName = "meshwhatever.Bundles.terminal.bundle";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Debugger.LogError("Resource 'terminal.bundle' not found in embedded resources.");
                        return null;
                    }

                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    return AssetBundle.LoadFromMemory(buffer);
                }
            }
            catch (Exception ex)
            {
                Debugger.LogError("Error loading terminal: " + ex.Message);
                return null;
            }
        }
        public static AssetBundle LoadEnvyDLDev()
        {
            try
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                string resourceName = "meshwhatever.Bundles.envydl_devmode.bundle";
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        Debugger.LogError("Resource 'envydl_devmode.bundle' not found in embedded resources.");
                        return null;
                    }

                    byte[] buffer = new byte[stream.Length];
                    stream.Read(buffer, 0, buffer.Length);
                    return AssetBundle.LoadFromMemory(buffer);
                }
            }
            catch (Exception ex)
            {
                Debugger.LogError("Error loading envydl_devmode: " + ex.Message);
                return null;
            }
        }
    }
}
