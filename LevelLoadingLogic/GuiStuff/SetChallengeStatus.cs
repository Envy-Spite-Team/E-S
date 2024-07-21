using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DoomahLevelLoader
{
    public class SetChallengeStatus : MonoBehaviour
    {
        public bool Active;

        public void Awake()
        {
            if (Active)
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