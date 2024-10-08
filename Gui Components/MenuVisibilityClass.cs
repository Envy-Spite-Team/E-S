﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using TMPro;

namespace DoomahLevelLoader
{
    public class MenuVisibilityClass : MonoBehaviour
    {
        private static MenuVisibilityClass instance;

        public Button AssignToMenuOpenButton;
        public Button AssignToGoBackButton;
        public GameObject AssignToFuckingLevels;

        public static MenuVisibilityClass Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<MenuVisibilityClass>();
                    if (instance == null)
                        Debugger.LogError("WOWZERS! SOMEONE FORGOT TO FIX THIS!"); //WOWZERS WHAT THE FUCK DOES THIS MEAN :D --triggered
                }
                return Instance;
            }
        }

        private void Start()
        {
            AssignToMenuOpenButton.onClick.AddListener(ToggleMenu);
            AssignToGoBackButton.onClick.AddListener(ToggleMenu);
        }

        private void ToggleMenu()
        {
            if (AssignToFuckingLevels.activeSelf == false)
            {
                AssignToFuckingLevels.SetActive(true);
                AssignToMenuOpenButton.gameObject.SetActive(false);
                MainMenuAgony.isAgonyOpen = true;
                Debugger.Log("open");
            }
            else
            {
                AssignToFuckingLevels.SetActive(false);
                AssignToMenuOpenButton.gameObject.SetActive(true);
                MainMenuAgony.isAgonyOpen = false;
                Debugger.Log("close");
            }
        }
    }
}