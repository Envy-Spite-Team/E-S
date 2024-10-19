using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EnvyLevelLoader.Envy_UI
{
    [RequireComponent(typeof(TMP_Dropdown))]
    public class DifficultyDropdown : MonoBehaviour
    {
        public TMP_Dropdown dropdown;

        private const string selectedDifficultyKey = "difficulty";
        private string settingsFilePath;

        private void Awake()
        {
            if(dropdown == null)
                dropdown = gameObject.GetComponent<TMP_Dropdown>();
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
            Debugger.Log("Difficulty set to " + index);
            MonoSingleton<PrefsManager>.Instance.SetInt("difficulty", index);
        }
    }
}
