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
	public class ScriptWarningUI : MonoBehaviour
	{
		private static ScriptWarningUI instance;

		public Button YesButton;
		public Button NoButton;
		public TextMeshProUGUI ScriptHashText;

		public static ScriptWarningUI Instance
		{
			get
			{
				if (instance == null)
				{
					instance = FindObjectOfType<ScriptWarningUI>();
					if (instance == null)
					{
						Debug.LogError("EnvyScreen prefab not found or ScriptWarningUI instance not initialized.");
						return null;
					}
				}
				return Instance;
			}
		}

		public void Start()
		{

		}
	}
}