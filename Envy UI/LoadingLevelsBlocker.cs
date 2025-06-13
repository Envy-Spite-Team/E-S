using TMPro;
using UnityEngine;

namespace EnvyLevelLoader.UI
{
    public class LoadingLevelsBlocker : MonoBehaviour
    {
        public TextMeshProUGUI Title;
        public TextMeshProUGUI Text;

        public void AddItem(string item)
        {
            Text.text += item;
        }
    }
}