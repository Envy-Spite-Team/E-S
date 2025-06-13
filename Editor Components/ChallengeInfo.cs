using System;
using EnvyLevelLoader;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// totally didn't forget to add this or anything

namespace DoomahLevelLoader
{
    public class ChallengeInfo : MonoBehaviour
    {
        public string Challenge;
        public bool ActiveByDefault;

        [HideInInspector]
        public GameObject ChallengeText;

        public static bool HasRanThisScene { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        private static void Hook()
        {
            SceneManager.sceneLoaded += (scene, mode) =>
            {
                HasRanThisScene = false;
            };
        }

        public void OnDestroy()
        {
            HasRanThisScene = false;
        }

        public void Start()
        {
            ChallengeText = EnvyUtility.FindObjectEvenIfDisabled("Player", "Main Camera/HUD Camera/HUD/FinishCanvas/Panel/Challenge/ChallengeText");
            if (ChallengeText == null)
            {
                // double check
                ChallengeText = NewMovement.Instance?.transform.Find("Main Camera/HUD Camera/HUD/FinishCanvas/Panel/Challenge/ChallengeText")?.gameObject;
                if (ChallengeText == null) // keep going till it is not null
                {
                    Invoke(nameof(Start), 0.125f);
                    return;
                }
            }
            ChallengeText.GetComponent<TextMeshProUGUI>().text = Challenge;
            if (ActiveByDefault)
            {
                MonoSingleton<ChallengeManager>.Instance!.challengeFailed = false;
                MonoSingleton<ChallengeManager>.Instance!.challengeDone = true;
            }
            else
            {
                MonoSingleton<ChallengeManager>.Instance!.challengeFailed = true;
                MonoSingleton<ChallengeManager>.Instance!.challengeDone = false;
            }
            HasRanThisScene = true;
        }
    }
}