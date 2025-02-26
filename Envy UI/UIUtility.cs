using EnvyLevelLoader;
using EnvyLevelLoader.Loaders;
using UnityEngine;

public class UIUtility : MonoBehaviour
{
    public void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/RY8J67neJ9");
    }
    public void OpenLevelsFolder()
    {
        Application.OpenURL("file://" + EnvyUtility.ConfigPath.Replace("\\", "/"));
    }

    public void OpenCredits()
    {
        
    }
}