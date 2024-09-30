using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;

namespace DoomahLevelLoader
{
    public class LevelPopUp : MonoBehaviour
    {
        private static LevelPopUp instance;

        public TextMeshProUGUI LevelName;
        public TextMeshProUGUI LevelDescription;

        public static LevelPopUp Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<LevelPopUp>();
                    if (instance == null)
                    {
                        Debugger.LogError("EnvyScreen prefab not found or LevelPopUp instance not initialized.");
                        return null;
                    }
                }
                return Instance;
            }
        }
    }
}