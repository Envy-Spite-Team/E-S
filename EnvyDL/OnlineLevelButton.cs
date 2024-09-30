using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine.Networking;
using System.Collections;

namespace DoomahLevelLoader
{
    public class OnlineLevelButton : MonoBehaviour
    {
        public TextMeshProUGUI LevelName;
        public TextMeshProUGUI LevelAuthor;
        public GameObject CustomScriptIndicator;
        public Button InstallButton;
        public Button AboutButton;
        public Image ActualImage;
        public TextMeshProUGUI SeriousOrSatire;

        [HideInInspector]
        public string LevelDescription;
    }
}