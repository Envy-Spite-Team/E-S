
using UnityEngine;

namespace DoomahLevelLoader;

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
}