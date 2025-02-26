using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace EnvyLevelLoader.UI
{
    public class MOTDHook : MonoBehaviour
    {
        public void Start()
        {
            StartCoroutine(MOTDManager.LoadMOTD(GetComponent<TextMeshProUGUI>(), new GameObject().AddComponent<Image>()));
        }
    }
}