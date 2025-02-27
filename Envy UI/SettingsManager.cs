using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace EnvyLevelLoader.UI
{
    public class EnvySettingsManager : MonoBehaviour
    {
        [Header("Settings")]
        public Toggle enableCircuitBackground;
        public Toggle enableLoadingScreen;
        public Toggle showMessageOfTheDay;
        public Toggle patchNewEnemies; // TODO : implement
        public TMP_InputField windowTitleInput;

        [Header("Other")]
        public string defaultWindowTitle;
        public TextMeshProUGUI windowTitle;
        public GameObject messageOfTheDay;
        public GameObject circuitBackground;

        internal static bool GetBool(string key, bool defaultValue)
        {
            var pm = MonoSingleton<PrefsManager>.Instance;
            if (pm == null)
            {
                Debugger.LogWarn("Prefs manager is null!");
                return defaultValue;
            }
            key = "envyloader__" + key;
            return pm.GetBoolLocal(key, defaultValue);
        }
        internal static string GetString(string key, string defaultValue)
        {
            var pm = MonoSingleton<PrefsManager>.Instance;
            if (pm == null)
            {
                Debugger.LogWarn("Prefs manager is null!");
                return defaultValue;
            }
            key = "envyloader__" + key;
            return pm.GetStringLocal(key, defaultValue);
        }
        
        internal static void SetBool(string key, bool value)
        {
            var pm = MonoSingleton<PrefsManager>.Instance;
            if (pm == null)
            {
                Debugger.LogWarn("Prefs manager is null!");
                return;
            }
            key = "envyloader__" + key;
            pm.SetBoolLocal(key, value);
        }
        internal static void SetString(string key, string value)
        {
            var pm = MonoSingleton<PrefsManager>.Instance;
            if (pm == null)
            {
                Debugger.LogWarn("Prefs manager is null!");
                return;
            }
            key = "envyloader__" + key;
            pm.SetStringLocal(key, value);
        }

        public void LoadSettings()
        {
            enableCircuitBackground.isOn = GetBool("enableCircuitBackground", true);
            showMessageOfTheDay.isOn = GetBool("showMessageOfTheDay", true);
            windowTitleInput.text = GetString("windowTitleInput", defaultWindowTitle);
            enableLoadingScreen.isOn = GetBool("enableLoadingScreen", true);
            patchNewEnemies.isOn = GetBool("patchNewEnemies", true);
        }

        public void SaveSettings()
        {
            SetBool("enableCircuitBackground", enableCircuitBackground.isOn);
            SetBool("showMessageOfTheDay", showMessageOfTheDay.isOn);
            SetString("windowTitleInput", windowTitleInput.text);
            SetBool("enableLoadingScreen", enableLoadingScreen.isOn);
            SetBool("patchNewEnemies", patchNewEnemies.isOn);
        }

        public void UpdateElements()
        {
            windowTitle.text = windowTitleInput.text;
            if(circuitBackground.activeSelf != enableCircuitBackground.isOn)
                circuitBackground.SetActive(enableCircuitBackground.isOn);
            if(messageOfTheDay.activeSelf != showMessageOfTheDay.isOn)
                messageOfTheDay.SetActive(showMessageOfTheDay.isOn);
            if(string.IsNullOrWhiteSpace(windowTitleInput.text))
                windowTitleInput.text = defaultWindowTitle;
            
            SaveSettings();
        }
        
        public void OnEnable()
        {
            LoadSettings();
            UpdateElements();
        }

        public void OnDisable()
        {
            SaveSettings();
        }

        public void OnDestroy()
        {
            SaveSettings();
        }
        
        public void Update()
        {
            UpdateElements();
        }
    }
}