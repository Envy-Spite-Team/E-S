using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using Newtonsoft.Json;
using System.Reflection;
using UnityEngine.Networking;
using System.Threading;

namespace DoomahLevelLoader
{
public class EnvyLoaderMenu : MonoBehaviour
{
	private static EnvyLoaderMenu instance;

	public GameObject ContentStuff;
	public GameObject LevelsMenu;
	public GameObject LevelsButton;
	public GameObject FuckingPleaseWait;
	public TextMeshProUGUI MOTDMessage;

	private const string motdUrl = "https://raw.githubusercontent.com/SatisfiedBucket/EnvySpiteDownloader/main/MOTD.txt";

	public static EnvyLoaderMenu Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<EnvyLoaderMenu>();
				if (instance == null)
				{
					Debug.LogError("EnvyScreen prefab not found or EnvyLoaderMenu instance not initialized.");
					return null;
				}
			}
			return instance;
		}
	}

	public void Start()
	{
		EnvyLoaderMenu.CreateLevels();
		StartCoroutine(LoadMOTD());
	}
	
	private IEnumerator LoadMOTD()
	{
		using (UnityWebRequest webRequest = UnityWebRequest.Get(motdUrl))
		{
			yield return webRequest.SendWebRequest();

			if (webRequest.isNetworkError || webRequest.isHttpError)
			{
				Debug.LogError("Failed to fetch MOTD: " + webRequest.error);
			}
			else
			{
				string motdText = webRequest.downloadHandler.text;
				if (!string.IsNullOrEmpty(motdText))
				{
					string[] lines = motdText.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
					string firstLine = lines.Length > 0 ? lines[0] : "No MOTD available";

					MOTDMessage.text = firstLine;
				}
			}
		}
	}

	public static void CreateLevels()
	{
		foreach (AssetBundleInfo bundleInfo in Loaderscene.AssetBundles)
		{
			GameObject levelButtonObject = Instantiate(Instance.LevelsButton, Instance.ContentStuff.transform);
			LevelButtonScript buttonScript = levelButtonObject.GetComponent<LevelButtonScript>();

			Loaderscene.SetLevelButtonScriptProperties(buttonScript, bundleInfo);

			if (bundleInfo.IsCampaign)
			{
				buttonScript.SceneToLoad = "";
			}

            buttonScript.LevelButtonReal.onClick.AddListener(() => Loaderscene.LoadScene(buttonScript));
        }
	}

	public static void ClearContentStuffChildren()
	{
		if (Instance == null) return;

		foreach (Transform child in Instance.ContentStuff.transform)
		{
			Destroy(child.gameObject);
		}
	}

	public static IEnumerator UpdateLevelListingCoroutine()
	{
		Instance.FuckingPleaseWait.SetActive(true);

		ClearContentStuffChildren();
		CreateLevels();

		yield return null;

		Instance.FuckingPleaseWait.SetActive(false);
	}

	public static void UpdateLevelListing()
	{
		if (Instance != null)
		{
			Instance.StartCoroutine(UpdateLevelListingCoroutine());
		}
	}
}

public class DropdownHandler : MonoBehaviour
{
	public TMP_Dropdown dropdown;

	private const string selectedDifficultyKey = "difficulty";
	private string settingsFilePath;

	private void Awake()
	{
		settingsFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "settings.json");
	}

	private void OnEnable()
	{
		int savedDifficulty = LoadDifficulty();
		dropdown.value = savedDifficulty;
		dropdown.onValueChanged.AddListener(OnDropdownValueChanged);
	}

	private int LoadDifficulty()
	{
            return MonoSingleton<PrefsManager>.Instance.GetInt("difficulty");
    }

	public void OnDropdownValueChanged(int index)
	{
			Debug.Log("Difficulty set to " + index);
            MonoSingleton<PrefsManager>.Instance.SetInt("difficulty", index);
    }
}
}