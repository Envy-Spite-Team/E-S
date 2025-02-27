using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using EnvyLevelLoader;
using EnvyLevelLoader.Loaders;
using EnvyLevelLoader.Parsers;
using EnvyLevelLoader.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using Object = UnityEngine.Object;

public class UIUtility : MonoBehaviour
{
    public void OpenDiscord()
    {
        Application.OpenURL("https://discord.gg/RY8J67neJ9");
    }
    public void OpenLevelsFolder()
    {
        if (!Directory.Exists(EnvyUtility.ConfigPath))
            Directory.CreateDirectory(EnvyUtility.ConfigPath);
        
        Application.OpenURL("file://" + EnvyUtility.ConfigPath.Replace("\\", "/"));
    }

    private const int loadingSize = 24;
    static IEnumerator OpenCreditsLoadingScreen(AsyncOperation asyncOp, LoadingLevelsBlocker info)
    {
        while (!asyncOp.isDone)
        {
            yield return null;
            if (info != null)
            {
                info.Text.text = "[";
                for (int i = 0; i < loadingSize * (asyncOp.progress); i++)
                {
                    info.Text.text += "*";
                }

                for (int i = 0; i < loadingSize * (1 - (asyncOp.progress)); i++)
                {
                    info.Text.text += "-";
                }
                info.Text.text += "]";
            }
        }

        if (info != null)
        {
            Object.Destroy(info.GetComponentInParent<Transform>()!.gameObject);
        }
    }
    public void OpenCredits()
    {
        if (!Directory.Exists(EnvyUtility.ConfigPath))
            Directory.CreateDirectory(EnvyUtility.ConfigPath);
        if (!File.Exists(EnvyUtility.CreditsLevelPath))
        {
            if(Application.internetReachability == NetworkReachability.NotReachable) return;
            UnityWebRequest wwr = UnityWebRequest.Get("https://file.garden/Z5hylsSV6nNdNxZi/creditsE%26S.doomah");
            
            var loadingScreenOG = Plugin.menu.LoadAsset<GameObject>("LoadingALevelBlocker");
            LoadingLevelsBlocker info = null;
            if (loadingScreenOG != null)
            {
                var canvasForEnvyInstance = UnityEngine.Object.Instantiate(Plugin.canvasForEnvy, null);
                var loadingScreen = UnityEngine.Object.Instantiate(loadingScreenOG, canvasForEnvyInstance.transform);
                loadingScreen.SetActive(true);
                info = loadingScreen.GetComponentInChildren<LoadingLevelsBlocker>();
                info.Title.text = "Downloading credits level";
            }
            
            var rq = wwr.SendWebRequest();
            StartCoroutine(OpenCreditsLoadingScreen(rq, info));
            rq.completed += (rq) =>
            {
                if(wwr.isNetworkError || wwr.isHttpError)
                    return;
                File.WriteAllBytes(EnvyUtility.CreditsLevelPath, wwr.downloadHandler.data);
                EnvyUtility.RunOnMainThread(() =>
                {
                    Debugger.Log("Credits level downloaded, now trying to open...");
                    OpenCredits();
                }, 0.125f);
            };
            return;
        }

        using (ZipArchive archive = ZipFile.OpenRead(EnvyUtility.CreditsLevelPath))
        {
            EnvyLevel credits = null;
            try
            {
                credits = DoomahParser.ParseLevelInfo(archive);
                credits.FilePath = EnvyUtility.CreditsLevelPath;
            }catch(Exception) {return;}

            string levelKey = LevelLoader.GetLevelKey(credits);
            SceneHelper.LoadScene(levelKey);
        }
    }

    public void SetDate(TextMeshProUGUI dateTarget)
    {
        // Get the current culture (locale) of the user
        CultureInfo culture = CultureInfo.CurrentCulture;

        // Get the short date format from the culture info
        string dateFormat = culture.DateTimeFormat.ShortDatePattern;

        // Get today's date in the user's format
        string formattedDate = DateTime.Now.ToString(dateFormat);
        dateTarget.text = formattedDate.ToUpper();
    }
}