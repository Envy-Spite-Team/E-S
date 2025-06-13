using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using Debug = UnityEngine.Debug;

namespace EnvyLevelLoader
{
    /// <summary>
    /// Used to log stuff that should only be logged in a developer environment.
    /// </summary>
    public static class Debugger {

        // funny story, i spent ages figuring out why logging wasnt working
        // until i found out debug.logs only work on main thread
        // which is why EnvyUtility.RunOnMainThread is a thing now
        
        [Conditional("DEBUG")]
        public static void Log(object messageRaw,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string file = null)
        {
            string message = $"[{Path.GetFileName(file)}:{lineNumber}] {messageRaw}";
            
            if(!EnvyUtility.IsMainThread)
            { EnvyUtility.RunOnMainThread(() => Debug.Log(message)); return; }

            Debug.Log(message);
        }
        
        [Conditional("DEBUG")]
        public static void LogWarn(object messageRaw,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string file = null)
        {
            string message = $"[{Path.GetFileName(file)}:{lineNumber}] {messageRaw}";

            if (!EnvyUtility.IsMainThread)
            { EnvyUtility.RunOnMainThread(() => Debug.LogWarning(message)); return; }

            Debug.LogWarning(message);
        }
        
        [Conditional("DEBUG")]
        public static void LogError(object messageRaw,
            [CallerLineNumber] int lineNumber = 0,
            [CallerFilePath] string file = null)
        {
            string message = $"[{Path.GetFileName(file)}:{lineNumber}] {messageRaw}";

            if (!EnvyUtility.IsMainThread)
            { EnvyUtility.RunOnMainThread(() => Debug.LogError(message)); return; }

            Debug.LogError(message);
        }

        static Dictionary<string,int> line_debugger = new Dictionary<string,int>();
        /// <summary>
        /// Used as a quick and dirty way to log "lines" of code.
        /// </summary>
        /// <param name="uuid_for_session">Used to keep track of how many times the function has been called, used to print the amount of times.</param>
        /// <param name="note">Gets tagged on at the end of the debug log.</param>
        [Conditional("DEBUG")]
        public static void LogLine(string uuid_for_session, string note = "")
        {
            if (!line_debugger.ContainsKey(uuid_for_session))
            { line_debugger[uuid_for_session] = 0; Log($"({uuid_for_session}) debug has started!"); }

            line_debugger[uuid_for_session]++;
            Log($"({uuid_for_session}) debug: {line_debugger[uuid_for_session]} note: {note}");
        }
    }
}