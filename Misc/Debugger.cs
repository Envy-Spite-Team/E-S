
using System.Collections.Generic;
using UnityEngine;

namespace EnvyLevelLoader
{
    /// <summary>
    /// Used to log stuff that should only be logged in a developer environment.
    /// </summary>
    public static class Debugger {
        public static void Log(object message)
        {
#if DEBUG
            Debug.Log(message);
#endif
        }
        public static void LogWarn(object message)
        {
#if DEBUG
            Debug.LogWarning(message);
#endif
        }
        public static void LogError(object message)
        {
#if DEBUG
            Debug.LogError(message);
#endif
        }

#if DEBUG
        static Dictionary<string,int> line_debugger = new Dictionary<string,int>();
#endif
        /// <summary>
        /// Used as a quick and dirty way to log "lines" a function.
        /// </summary>
        /// <param name="uuid_for_session">Used to keep track of how many times the function has been called, used to print the amount of times.</param>
        /// <param name="note">Gets tagged on at the end of the debug log.</param>
        public static void LogLine(string uuid_for_session, string note = "")
        {
#if DEBUG
            if (!line_debugger.ContainsKey(uuid_for_session))
            { line_debugger[uuid_for_session] = 0; Log($"({uuid_for_session}) debug has started!"); }

                line_debugger[uuid_for_session]++;
            Log($"({uuid_for_session}) debug: {line_debugger[uuid_for_session]} note: {note}");
#endif
        }
    }
}