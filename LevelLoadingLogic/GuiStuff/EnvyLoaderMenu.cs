using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;
using Newtonsoft.Json;
using System.Reflection;
using System.Net.Http;
using System.Threading.Tasks;
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
	private bool AlreadyStarted = false;

	public static EnvyLoaderMenu Instance
	{
		get
		{
			if (instance == null)
			{
				instance = FindObjectOfType<EnvyLoaderMenu>();
				if (instance == null)
				{
					Debug.LogError("EnvyScreen prefab not found in the terminal bundle. This error may look scary but it's probably nothing to worry about.");
				}
			}
			if (instance.AlreadyStarted == false)
			{
				instance.AlreadyStarted = true;
				instance.Start();
			}
			return instance;
		}
	}

	public void Start()
	{
		EnvyLoaderMenu.CreateLevels();
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
			
			if (!string.IsNullOrEmpty(buttonScript.SceneToLoad))
			{
				buttonScript.LevelButtonReal.onClick.AddListener(() => Loaderscene.LoadScene(buttonScript));
			}
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
		if (File.Exists(settingsFilePath))
		{
			string json = File.ReadAllText(settingsFilePath);
			Dictionary<string, int> settings = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);

			if (settings != null && settings.TryGetValue(selectedDifficultyKey, out int difficulty))
			{
				return difficulty;
			}
		}
		return 2;
	}

	private void SaveDifficulty(int difficulty)
	{
		Dictionary<string, int> settings = new Dictionary<string, int>
		{
			{ selectedDifficultyKey, difficulty }
		};

		string json = JsonConvert.SerializeObject(settings, Formatting.Indented);
		File.WriteAllText(settingsFilePath, json);
	}

	public void OnDropdownValueChanged(int index)
	{
		SaveDifficulty(index);
	}
}
}