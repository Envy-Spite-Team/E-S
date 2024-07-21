﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

namespace DoomahLevelLoader
{
    public class EnvyandSpiteterimal : MonoBehaviour
    {
        private static EnvyandSpiteterimal instance;

        public TextMeshProUGUI levelname;
        public TextMeshProUGUI loadbutton;
        public Button load;
        public Button gofowardinlist;
        public Button gobackinlist;
        public Image Levelpicture;
        public TextMeshProUGUI FrownyFace;
        public GameObject FuckingPleaseWait;
        public Button Discord;

        public static EnvyandSpiteterimal Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<EnvyandSpiteterimal>();

                    if (instance == null)
                    {
                        Debug.LogError("EnvyandSpiteterimal instance not found in the scene.");
                    }
                }
                return instance;
            }
        }

        private void Start()
        {
            load.onClick.AddListener(OnLoadButtonClick);

            gofowardinlist.onClick.AddListener(OnGoForwardButtonClick);
            gobackinlist.onClick.AddListener(OnGoBackButtonClick);
			
			Levelpicture.color = Color.white;
			
            UpdateLevelName();
			Loaderscene.UpdateLevelPicture(Levelpicture, FrownyFace);

            Discord.onClick.AddListener(OnDiscordButtonClick);
        }

        private void OnLoadButtonClick()
        {
            Loaderscene.Loadscene();
        }

        private void OnGoForwardButtonClick()
        {
            Loaderscene.MoveToNextAssetBundle();
			Loaderscene.ExtractSceneName();
            UpdateLevelName();
			Loaderscene.UpdateLevelPicture(Levelpicture, FrownyFace);
        }

        private void OnGoBackButtonClick()
        {
            Loaderscene.MoveToPreviousAssetBundle();
			Loaderscene.ExtractSceneName();
            UpdateLevelName();
			Loaderscene.UpdateLevelPicture(Levelpicture, FrownyFace);
        }

        private void OnDiscordButtonClick()
        {
            Application.OpenURL("https://discord.gg/RY8J67neJ9");
        }

		public void UpdateLevelName()
		{
			string bundleFolderPath = Loaderscene.GetCurrentBundleFolderPath();

			if (string.IsNullOrEmpty(bundleFolderPath))
			{
				return;
			}

			string infoFilePath = Path.Combine(bundleFolderPath, "info.txt");

			if (!File.Exists(infoFilePath))
			{
				levelname.text = "Failed to load Level name!";
				return;
			}

			try
			{
				string[] lines = File.ReadAllLines(infoFilePath);

				if (lines.Length >= 2)
				{
					string levelName = lines[1];
					levelname.text = levelName;
				}
				else
				{
					levelname.text = "Failed to load Level name!";
				}
			}
			catch
			{
				levelname.text = "Failed to load Level name!";
			}
		}
    }
}
