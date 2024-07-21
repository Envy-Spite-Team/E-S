using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoomahLevelLoader
{
    public class ChallengeInfo : MonoBehaviour
    {
        public string Challenge;
        public bool ActiveByDefault;

        [HideInInspector]
        public GameObject ChallengeText;

        public void Awake()
        {
            ChallengeText = Plugin.FindObjectEvenIfDisabled("Player", "Main Camera/HUD Camera/HUD/FinishCanvas/Panel/Challenge/ChallengeText");
            ChallengeText.GetComponent<TextMeshProUGUI>().text = Challenge;
            if (ActiveByDefault)
            {
                MonoSingleton<ChallengeManager>.Instance.challengeFailed = false;
                MonoSingleton<ChallengeManager>.Instance.challengeDone = true;
            }
            else
            {
                MonoSingleton<ChallengeManager>.Instance.challengeFailed = true;
                MonoSingleton<ChallengeManager>.Instance.challengeDone = false;
            }
        }
    }
}